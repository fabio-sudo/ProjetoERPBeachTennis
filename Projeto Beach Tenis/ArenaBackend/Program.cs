using ArenaBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ArenaBackend.Middlewares;
using ArenaBackend.Repositories;
using ArenaBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=DESKTOP-HEJGC8G\\SQLEXPRESS;Database=ArenaManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddDbContext<ArenaDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<ArenaBackend.Services.IAnalyticsService, ArenaBackend.Services.AnalyticsService>();
builder.Services.AddScoped<ArenaBackend.Services.ITabsService, ArenaBackend.Services.TabsService>();

// Dependency Injection for new modules
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Phase 4 Services
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddHostedService<BillingBackgroundService>();

// Configure JWT Authentication
var secretKey = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForBeachTennisAndFutvoleiAppDontShare";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure CORS for local frontend testing
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Serve Frontend Static Files
var frontendPath = Path.Combine(builder.Environment.ContentRootPath, "..", "ArenaFrontend");
if (Directory.Exists(frontendPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        DefaultFileNames = new List<string> { "pages/index.html" }
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath)
    });
}

// Redirect root URL to index.html (served by static files) or Swagger as fallback
app.MapGet("/", (HttpContext context) =>
{
    if (Directory.Exists(frontendPath))
    {
        var indexPath = Path.Combine(frontendPath, "pages", "index.html");
        if (File.Exists(indexPath)) return Results.Redirect("/pages/index.html");
    }
    return Results.Redirect("/swagger");
});

// Add Global Error Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArenaDbContext>();
    var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Administrador");
    if (adminRole == null)
    {
        adminRole = new ArenaBackend.Models.Role { Name = "Administrador" };
        context.Roles.Add(adminRole);
        context.SaveChanges();
    }

    if (!context.Users.Any(u => u.Username == "admin"))
    {
        context.Users.Add(new ArenaBackend.Models.User
        {
            Name = "Administrador Padrão",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            RoleId = adminRole.Id,
            Active = true
        });
        context.SaveChanges();
    }
}

app.Run();