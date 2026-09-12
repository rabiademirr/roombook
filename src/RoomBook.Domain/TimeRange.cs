namespace RoomBook.Domain;

public readonly record struct TimeRange(DateTimeOffset Start, DateTimeOffset End)
{
    public TimeSpan Duration => End - Start;
}
