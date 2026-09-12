using RoomBook.Domain;

namespace RoomBook.Domain.Tests;

public class FreeSlotFinderTests
{
    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    [Fact]
    public void FindNearest_ReturnsUpTo3NearestSlots_OrderedByDistanceThenLaterOnTie()
    {
        var noExistingBookings = Array.Empty<TimeRange>();
        var requestedStart = Utc(12, 0);
        var duration = TimeSpan.FromMinutes(30);

        var result = FreeSlotFinder.FindNearest(noExistingBookings, requestedStart, duration);

        Assert.Equal(3, result.Count);
        Assert.Equal(Utc(12, 0), result[0].Start);
        Assert.Equal(Utc(12, 15), result[1].Start);
        Assert.Equal(Utc(11, 45), result[2].Start);
    }

    [Fact]
    public void FindNearest_NoFreeSlotInDay_ReturnsEmptyList()
    {
        var fullyBooked = new[] { new TimeRange(Utc(9, 0), Utc(18, 0)) };
        var requestedStart = Utc(12, 0);
        var duration = TimeSpan.FromMinutes(30);

        var result = FreeSlotFinder.FindNearest(fullyBooked, requestedStart, duration);

        Assert.Empty(result);
    }
}
