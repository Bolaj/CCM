using System.Threading.Channels;
using CityChoir.Application.DTOs.Rehearsal;
using CityChoir.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CityChoir.Infrastructure.Services;

public class RehearsalNotificationWorker : BackgroundService, IRehearsalNotificationQueue
{
    private readonly Channel<RehearsalNotificationDto> _queue =
        Channel.CreateUnbounded<RehearsalNotificationDto>();
    private readonly IServiceScopeFactory _scopeFactory;

    public RehearsalNotificationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public ValueTask EnqueueAsync(
        RehearsalNotificationDto rehearsal,
        CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(rehearsal, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var rehearsal in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var members = await userRepository.GetActiveMembers();
                var emailTasks = members.Select(member => emailService.SendEmailAsync(
                    member.Email,
                    "New rehearsal scheduled",
                    BuildEmailBody(rehearsal)));

                await Task.WhenAll(emailTasks);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Rehearsal notification failed: {exception.Message}");
            }
        }
    }

    private static string BuildEmailBody(RehearsalNotificationDto rehearsal)
    {
        return $@"
            <p>A new rehearsal has been created:</p>
            <ul>
                <li><strong>{rehearsal.Name}</strong></li>
                <li>{rehearsal.Description}</li>
                <li>When: {rehearsal.RehearsalDate:MMMM dd, yyyy}</li>
                <li>Start: {rehearsal.StartTime:HH:mm}</li>
                <li>End: {rehearsal.EndTime:HH:mm}</li>
                <li>Venue: {rehearsal.Venue}</li>
            </ul>
        ";
    }
}