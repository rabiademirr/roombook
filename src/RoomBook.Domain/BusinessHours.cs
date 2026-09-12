namespace RoomBook.Domain;

public static class BusinessHours
{
    public static readonly TimeSpan StartOfDay = TimeSpan.FromHours(9);
    public static readonly TimeSpan EndOfDay = TimeSpan.FromHours(18);

    public static bool Contains(TimeRange range)
    {
        var start = range.Start.UtcDateTime;
        var end = range.End.UtcDateTime;
        return start.Date == end.Date
            && start.TimeOfDay >= StartOfDay
            && end.TimeOfDay <= EndOfDay;
    }
}
