using CityChoir.Domain.Enums;

namespace CityChoir.Domain.Entities;

public class EmailNotification
{
    public long Id { get; set; }
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public EmailNotificationStatus Status { get; set; } = EmailNotificationStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
