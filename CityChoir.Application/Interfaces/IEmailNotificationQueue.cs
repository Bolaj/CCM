using CityChoir.Application.DTOs.Common;

namespace CityChoir.Application.Interfaces;

public interface IEmailNotificationQueue
{
    ValueTask EnqueueAsync(
        EmailNotificationDto notification,
        CancellationToken cancellationToken = default);
}