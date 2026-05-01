using CityChoir.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityChoir.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Occupation)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Part)
            .HasConversion<string>();
    }
}