using CityChoir.Application.Interfaces;
using CityChoir.Application.Services;
using CityChoir.Infrastructure.Data;
using CityChoir.Infrastructure.Repository;
using CityChoir.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using DotNetEnv;

Env.Load();


var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The City Choir API",
        Version = "v1"
    });
});

// DbContext
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

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

#endregion

var app = builder.Build();

#region Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "The City Choir API v1");
    });
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers(); 

#endregion

app.Run();