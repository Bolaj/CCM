namespace CityChoir.Application.DTOs.Rehearsal;

public class RehearsalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int RadiusMeters { get; set; }
    public DateTime RehearsalDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
