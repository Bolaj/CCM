using CityChoir.Domain.Entities;

namespace CityChoir.Application.Services;

internal static class RehearsalSchedule
{
    public static DateTime GetStart(Rehearsal rehearsal)
    {
        return DateTime.SpecifyKind(
            rehearsal.RehearsalDate.Date.Add(rehearsal.StartTime.TimeOfDay),
            DateTimeKind.Utc);
    }

    public static DateTime GetEnd(Rehearsal rehearsal)
    {
        var start = GetStart(rehearsal);
        var end = DateTime.SpecifyKind(
            rehearsal.RehearsalDate.Date.Add(rehearsal.EndTime.TimeOfDay),
            DateTimeKind.Utc);

        return end <= start ? end.AddDays(1) : end;
    }
}
