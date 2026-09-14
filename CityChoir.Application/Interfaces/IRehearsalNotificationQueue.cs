using CityChoir.Application.DTOs.Rehearsal;

namespace CityChoir.Application.Interfaces;

public interface IRehearsalNotificationQueue
{
    ValueTask EnqueueAsync(RehearsalNotificationDto rehearsal, CancellationToken cancellationToken = default);
}