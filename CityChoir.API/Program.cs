using System.Security.Claims;
using System.Text;
using CityChoir.Application.Interfaces;
using CityChoir.Application.Services;
using CityChoir.Infrastructure.Data;
using CityChoir.Infrastructure.Repository;
using CityChoir.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

#region Services

// 1. Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
        ),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };

        options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var claims = context.Principal?.Claims
                .Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("Token validated. Claims:");
            foreach (var claim in claims ?? [])
                Console.WriteLine($"  {claim}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"Challenge triggered: {context.Error} - {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// 2. Authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 3. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// 4. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The City Choir API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    });

    c.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        );
        return requirement;
    });
});

// 5. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DBConnectionString");
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

#endregion

#region Dependency Injection

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailTokenRepository, EmailTokenRepository>();
builder.Services.AddScoped<IRehearsalRepository, RehearsalRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<EmailNotificationWorker>();
builder.Services.AddSingleton<IEmailNotificationQueue>(serviceProvider =>
    serviceProvider.GetRequiredService<EmailNotificationWorker>());
builder.Services.AddHostedService(serviceProvider =>
    serviceProvider.GetRequiredService<EmailNotificationWorker>());
builder.Services.AddSingleton<RehearsalNotificationWorker>();
builder.Services.AddSingleton<IRehearsalNotificationQueue>(serviceProvider =>
    serviceProvider.GetRequiredService<RehearsalNotificationWorker>());
builder.Services.AddHostedService(serviceProvider =>
    serviceProvider.GetRequiredService<RehearsalNotificationWorker>());
builder.Services.AddScoped<IRehearsalService, RehearsalService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

#endregion

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
    var smtpConnected = await emailService.TestConnectionAsync();

    if (smtpConnected)
        Console.WriteLine("SMTP startup check passed.");
    else
        Console.WriteLine("SMTP startup check failed. Email delivery will be retried at send time.");
}

#region Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "The City Choir API v1");
    });
    app.MapScalarApiReference(options =>
    {
        options.AddDocument(
            "v1",
            "The City Choir API",
            "/swagger/v1/swagger.json"
        );
    }).AllowAnonymous();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#endregion

app.Run();