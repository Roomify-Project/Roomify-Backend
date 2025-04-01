using Roomify.GP.Repository.Repositories;
using Roomify.GP.Service.Services;
using Roomify.GP.Service.Mappings;
using Roomify.GP.Service.Helpers;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Services.Contract;
using Roomify.GP.Core.Settings;
using Roomify.GP.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Roomify.GP.API.Middlewares;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Service;
using IJwtService = Roomify.GP.Service.Services.IJwtService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// Register Repositories & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPortfolioPostRepository, PortfolioPostRepository>();
builder.Services.AddScoped<IPortfolioPostService, PortfolioPostService>();
builder.Services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
Console.WriteLine($"🔍 JWT Loaded → SecretKey: {jwtSettings.SecretKey}");

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IJwtService, JwtService>();





builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register EmailService
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Load appsettings.Local.json if exists
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    //JsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);


// Register Cloudinary
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddSingleton(x =>
{
    var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();
    var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
    return new Cloudinary(account);
});

// Add Swagger (for API testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database Migrations and Role Creation
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<AppDbContext>();
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

try
{
    await context.Database.MigrateAsync();

    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

// Configure ApplicationUser-Defined Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
