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
    public DbSet<Rehearsal> Rehearsals { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<RegistrationSequence> RegistrationSequences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Occupation)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Part)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.RegistrationNumber)
            .IsUnique();

        modelBuilder.Entity<RegistrationSequence>()
            .HasKey(sequence => new { sequence.Year, sequence.Part });

        // Permission
        modelBuilder.Entity<Permission>()
            .Property(p => p.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Permission>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Permission>()
            .HasOne(p => p.Rehearsal)
            .WithMany()
            .HasForeignKey(p => p.RehearsalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Attendance
        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Rehearsal)
            .WithMany()
            .HasForeignKey(a => a.RehearsalId)
            .OnDelete(DeleteBehavior.Cascade);

        // prevent duplicate attendance per user per rehearsal
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.UserId, a.RehearsalId })
            .IsUnique();
    }
}