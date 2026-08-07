// Rehearsal entity
public class Rehearsal
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int RadiusMeters { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime RehearsalDate { get; set; }
}
