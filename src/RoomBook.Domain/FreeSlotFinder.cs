namespace RoomBook.Domain;

public static class FreeSlotFinder
{
    private static readonly TimeSpan Step = TimeSpan.FromMinutes(15);

    public static IReadOnlyList<TimeRange> FindNearest(
        IReadOnlyList<TimeRange> existingOnRoom,
        DateTimeOffset requestedStart,
        TimeSpan duration,
        int maxResults = 3)
    {
        var day = requestedStart.UtcDateTime.Date;
        var dayStart = new DateTimeOffset(day, TimeSpan.Zero) + BusinessHours.StartOfDay;
        var dayEnd = new DateTimeOffset(day, TimeSpan.Zero) + BusinessHours.EndOfDay;

        var sorted = existingOnRoom
            .Where(b => b.Start < dayEnd && b.End > dayStart)
            .OrderBy(b => b.Start)
            .ToList();

        var gaps = new List<TimeRange>();
        var cursor = dayStart;
        foreach (var booking in sorted)
        {
            if (booking.Start > cursor)
                gaps.Add(new TimeRange(cursor, booking.Start));
            if (booking.End > cursor)
                cursor = booking.End;
        }
        if (cursor < dayEnd)
            gaps.Add(new TimeRange(cursor, dayEnd));

        var candidates = new List<TimeRange>();
        foreach (var gap in gaps)
        {
            for (var slotStart = gap.Start; slotStart + duration <= gap.End; slotStart += Step)
            {
                candidates.Add(new TimeRange(slotStart, slotStart + duration));
            }
        }

        return candidates
            .OrderBy(c => Math.Abs((c.Start - requestedStart).Ticks))
            .ThenByDescending(c => c.Start)
            .Take(maxResults)
            .ToList();
    }
}
