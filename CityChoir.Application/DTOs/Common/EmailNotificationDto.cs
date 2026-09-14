namespace CityChoir.Application.DTOs.Common;

public class EmailNotificationDto
{
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
}