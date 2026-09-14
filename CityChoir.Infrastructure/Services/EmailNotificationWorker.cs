using System.Threading.Channels;
using CityChoir.Application.DTOs.Common;
using CityChoir.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CityChoir.Infrastructure.Services;

public class EmailNotificationWorker : BackgroundService, IEmailNotificationQueue
{
    private readonly Channel<EmailNotificationDto> _queue =
        Channel.CreateUnbounded<EmailNotificationDto>();
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailNotificationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ValueTask EnqueueAsync(
        EmailNotificationDto notification,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(notification, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendEmailAsync(
                    notification.To,
                    notification.Subject,
                    notification.Body);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Email notification failed: {exception.Message}");
            }
        }
    }
}