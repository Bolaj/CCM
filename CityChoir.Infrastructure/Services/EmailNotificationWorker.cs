using CityChoir.Application.DTOs.Common;
using CityChoir.Application.Interfaces;
using CityChoir.Domain.Entities;
using CityChoir.Domain.Enums;
using CityChoir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CityChoir.Infrastructure.Services;

public class EmailNotificationWorker : BackgroundService, IEmailNotificationQueue
{
    private const int MaxRetryCount = 5;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailNotificationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async ValueTask EnqueueAsync(
        EmailNotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.EmailNotifications.AddAsync(new EmailNotification
        {
            To = notification.To,
            Subject = notification.Subject,
            Body = notification.Body
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingNotifications(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingNotifications(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var now = DateTime.UtcNow;

        var notifications = await dbContext.EmailNotifications
            .Where(notification => notification.Status == EmailNotificationStatus.Pending)
            .Where(notification => notification.NextAttemptAt <= now)
            .OrderBy(notification => notification.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            try
            {
                await emailService.SendEmailAsync(
                    notification.To,
                    notification.Subject,
                    notification.Body);

                notification.ProcessedAt = DateTime.UtcNow;
                dbContext.EmailNotifications.Remove(notification);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                notification.RetryCount++;
                notification.LastError = exception.Message;
                notification.NextAttemptAt = DateTime.UtcNow.AddMinutes(
                    Math.Min(Math.Pow(2, notification.RetryCount), 60));

                if (notification.RetryCount >= MaxRetryCount)
                    notification.Status = EmailNotificationStatus.Failed;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}