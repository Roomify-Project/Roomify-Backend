﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Roomify.GP.API.Hubs;
using Roomify.GP.API.Middlewares;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Services.Contract;
using Roomify.GP.Core.Settings;
using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Repository.Repositories;
using Roomify.GP.Service;
using Roomify.GP.Service.Helpers;
using Roomify.GP.Service.Mappings;
using Roomify.GP.Service.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using CloudinaryDotNet;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPortfolioPostRepository, PortfolioPostRepository>();
builder.Services.AddScoped<IPortfolioPostService, PortfolioPostService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserConnectionRepository, UserConnectionRepository>();
builder.Services.AddScoped<IUserConnectionService, UserConnectionService>();
builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddAuthorization(); // لازم جداً

// Identity
builder.Services.AddIdentityCore<ApplicationUser>(options => { })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();


// منع Redirect على /Account/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Cloudinary settings
builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var cloudinarySettings = config.GetSection("CloudinarySettings").Get<CloudinarySettings>();

    if (cloudinarySettings == null)
        throw new Exception("Cloudinary settings are not configured properly.");

    var account = new Account(cloudinarySettings.CloudName, cloudinarySettings.ApiKey, cloudinarySettings.ApiSecret);
    return new Cloudinary(account);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    });
});

// SignalR
builder.Services.AddSignalR();

// Swagger + JWT Auth
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Roomify API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Apply migrations and create roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    try
    {
        await context.Database.MigrateAsync();

        if (!await roleManager.RoleExistsAsync("NormalUser"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("NormalUser"));
        }
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred during migration.");
    }
}

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHub<PrivateChatHub>("/chat").RequireAuthorization();

app.Run();