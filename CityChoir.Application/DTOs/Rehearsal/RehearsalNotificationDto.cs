namespace CityChoir.Application.DTOs.Rehearsal;

public class RehearsalNotificationDto
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int RadiusMeters { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime RehearsalDate { get; set; }
}