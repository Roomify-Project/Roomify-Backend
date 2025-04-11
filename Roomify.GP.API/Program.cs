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
using Roomify.GP.API.Hubs;
using Roomify.GP.Core.Background_Services;

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
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
//builder.Services.AddScoped<IRoomImageRepository, RoomImageRepository>();
//builder.Services.AddScoped<IRoomImageService, RoomImageService>();
//builder.Services.AddScoped<IPromptRepository, PromptRepository>();
//builder.Services.AddSingleton<IAIResultHistoryRepository, AIResultHistoryRepository>();
//builder.Services.AddScoped<ISavedDesignRepository, SavedDesignRepository>();
//builder.Services.AddSingleton<IHostedService, CleanupService>();



builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Identity with Roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Register EmailService
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Load appsettings.Local.json if exists
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<RoleManager<IdentityRole<Guid>>>();  // Ensure RoleManager is correctly registered

// Register Cloudinary
builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();

    if (cloudinarySettings == null)
    {
        throw new Exception("Cloudinary settings are not configured properly.");
    }

    var account = new Account(cloudinarySettings.CloudName, cloudinarySettings.ApiKey, cloudinarySettings.ApiSecret);
    return new Cloudinary(account);
});

// Register The CleanupService as a BackgroundService
//builder.Services.AddHostedService<CleanupService>();

// Add SignalR
builder.Services.AddSignalR();

// Add Swagger (for API testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database Migrations and Role Creation
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<AppDbContext>();  
var loggerFactory = services.GetRequiredService<ILoggerFactory>();
var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

try
{
    await context.Database.MigrateAsync();

    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>("User"));
    }
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

// Configure ApplicationUser-Defined Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure CORS (Cross-Origin Resource Sharing)
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()  // السماح لجميع النطاقات
           .AllowAnyMethod()  // السماح باستخدام أي طريقة (GET, POST, PUT, DELETE, ...)
           .AllowAnyHeader(); // السماح باستخدام أي رأس (header)
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PrivateChatHub>("/chat"); // ربط الـ SignalR Hub مع URL

app.Run();
