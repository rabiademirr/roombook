using RoomBook.Domain;

namespace RoomBook.Domain.Tests;

public class BookingOverlapTests
{
    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    private static TimeRange Range(int startHour, int startMinute, int endHour, int endMinute) =>
        new(Utc(startHour, startMinute), Utc(endHour, endMinute));

    [Fact]
    public void Overlaps_IdenticalRanges_ReturnsTrue()
    {
        var existing = Range(9, 0, 10, 0);
        var requested = Range(9, 0, 10, 0);

        Assert.True(BookingOverlap.Overlaps(existing, requested));
    }

    [Fact]
    public void Overlaps_RequestOverlapsStartOfExisting_ReturnsTrue()
    {
        var existing = Range(9, 30, 10, 30);
        var requested = Range(9, 0, 10, 0);

        Assert.True(BookingOverlap.Overlaps(existing, requested));
    }

    [Fact]
    public void Overlaps_RequestOverlapsEndOfExisting_ReturnsTrue()
    {
        var existing = Range(9, 0, 10, 0);
        var requested = Range(9, 30, 10, 30);

        Assert.True(BookingOverlap.Overlaps(existing, requested));
    }

    [Fact]
    public void Overlaps_RequestFullyContainsExisting_ReturnsTrue()
    {
        var existing = Range(9, 15, 9, 45);
        var requested = Range(9, 0, 10, 0);

        Assert.True(BookingOverlap.Overlaps(existing, requested));
    }

    [Fact]
    public void Overlaps_BackToBackStartAtExistingEnd_ReturnsFalse()
    {
        var existing = Range(9, 0, 10, 0);
        var requested = Range(10, 0, 10, 30);

        Assert.False(BookingOverlap.Overlaps(existing, requested));
    }

    [Fact]
    public void Overlaps_BackToBackEndAtExistingStart_ReturnsFalse()
    {
        var existing = Range(10, 0, 10, 30);
        var requested = Range(9, 30, 10, 0);

        Assert.False(BookingOverlap.Overlaps(existing, requested));
    }

    [Fact]
    public void Overlaps_FarApart_ReturnsFalse()
    {
        var existing = Range(9, 0, 9, 30);
        var requested = Range(15, 0, 15, 30);

        Assert.False(BookingOverlap.Overlaps(existing, requested));
    }
}
