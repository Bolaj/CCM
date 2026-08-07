using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace CityChoir.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Try environment variable first
            var connectionString = Environment.GetEnvironmentVariable("DBConnectionString");

            // Fallback: try the API project's appsettings.json
            if (string.IsNullOrEmpty(connectionString))
            {
                var apiConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CityChoir.API", "appsettings.json");
                if (File.Exists(apiConfigPath))
                {
                    var json = File.ReadAllText(apiConfigPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                        cs.TryGetProperty("DBConnectionString", out var val))
                    {
                        connectionString = val.GetString();
                    }
                }
            }

            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Could not find a connection string named 'DBConnectionString'. Set the DBConnectionString environment variable or add it to CityChoir.API/appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
