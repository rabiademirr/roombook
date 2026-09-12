namespace RoomBook.Domain;

public sealed record Booking
{
    private static readonly TimeSpan MinDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(4);
    private static readonly TimeSpan AlignmentIncrement = TimeSpan.FromMinutes(15);

    public Guid Id { get; }
    public string RoomId { get; }
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }
    public string Organizer { get; }
    public string Title { get; }

    public TimeRange TimeRange => new(Start, End);

    private Booking(Guid id, string roomId, DateTimeOffset start, DateTimeOffset end, string organizer, string title)
    {
        Id = id;
        RoomId = roomId;
        Start = start;
        End = end;
        Organizer = organizer;
        Title = title;
    }

    public static bool TryCreate(
        string roomId,
        DateTimeOffset start,
        DateTimeOffset end,
        string organizer,
        string title,
        out Booking? booking,
        out IReadOnlyList<BookingRuleViolation> violations)
    {
        var found = new List<BookingRuleViolation>();

        if (start.Offset != TimeSpan.Zero || end.Offset != TimeSpan.Zero)
            found.Add(BookingRuleViolation.NotUtc);

        var duration = end - start;
        if (duration < MinDuration || duration > MaxDuration)
            found.Add(BookingRuleViolation.InvalidDuration);

        if (!IsAligned(start) || !IsAligned(end))
            found.Add(BookingRuleViolation.NotAligned);

        if (!BusinessHours.Contains(new TimeRange(start, end)))
            found.Add(BookingRuleViolation.OutsideBusinessHours);

        if (found.Count > 0)
        {
            booking = null;
            violations = found;
            return false;
        }

        booking = new Booking(Guid.NewGuid(), roomId, start, end, organizer, title);
        violations = Array.Empty<BookingRuleViolation>();
        return true;
    }

    private static bool IsAligned(DateTimeOffset t) =>
        t.UtcDateTime.TimeOfDay.Ticks % AlignmentIncrement.Ticks == 0;
}
