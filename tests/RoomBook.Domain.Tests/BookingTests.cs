using RoomBook.Domain;

namespace RoomBook.Domain.Tests;

public class BookingTests
{
    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    [Fact]
    public void TryCreate_ValidInput_Succeeds()
    {
        var ok = Booking.TryCreate("alpha", Utc(9, 0), Utc(9, 30), "Ada", "Standup", out var booking, out var violations);

        Assert.True(ok);
        Assert.NotNull(booking);
        Assert.Empty(violations);
    }

    [Theory]
    [InlineData(9, 0, 9, 15)]   // exactly 15 minutes, minimum valid duration
    [InlineData(9, 0, 13, 0)]   // exactly 4 hours, maximum valid duration
    public void TryCreate_DurationBoundaries_Succeed(int startHour, int startMinute, int endHour, int endMinute)
    {
        var ok = Booking.TryCreate("alpha", Utc(startHour, startMinute), Utc(endHour, endMinute), "Ada", "Standup", out _, out var violations);

        Assert.True(ok);
        Assert.Empty(violations);
    }

    [Fact]
    public void TryCreate_StartsBeforeBusinessHours_ReturnsOutsideBusinessHours()
    {
        var ok = Booking.TryCreate("alpha", Utc(8, 45), Utc(9, 0), "Ada", "Standup", out _, out var violations);

        Assert.False(ok);
        Assert.Contains(BookingRuleViolation.OutsideBusinessHours, violations);
    }

    [Fact]
    public void TryCreate_EndsAfterBusinessHours_ReturnsOutsideBusinessHours()
    {
        var ok = Booking.TryCreate("alpha", Utc(17, 45), Utc(18, 15), "Ada", "Standup", out _, out var violations);

        Assert.False(ok);
        Assert.Contains(BookingRuleViolation.OutsideBusinessHours, violations);
    }

    [Fact]
    public void TryCreate_ShorterThan15Minutes_ReturnsInvalidDuration()
    {
        var ok = Booking.TryCreate("alpha", Utc(9, 0), Utc(9, 0), "Ada", "Standup", out _, out var violations);

        Assert.False(ok);
        Assert.Contains(BookingRuleViolation.InvalidDuration, violations);
    }

    [Fact]
    public void TryCreate_LongerThan4Hours_ReturnsInvalidDuration()
    {
        var ok = Booking.TryCreate("alpha", Utc(9, 0), Utc(13, 15), "Ada", "Standup", out _, out var violations);

        Assert.False(ok);
        Assert.Contains(BookingRuleViolation.InvalidDuration, violations);
    }

    [Fact]
    public void TryCreate_NotAlignedTo15Minutes_ReturnsNotAligned()
    {
        var ok = Booking.TryCreate("alpha", Utc(9, 7), Utc(9, 22), "Ada", "Standup", out _, out var violations);

        Assert.False(ok);
        Assert.Contains(BookingRuleViolation.NotAligned, violations);
    }

    [Fact]
    public void TryCreate_NonUtcOffset_ReturnsNotUtc()
    {
        var start = new DateTimeOffset(2026, 9, 14, 9, 0, 0, TimeSpan.FromHours(2));
        var end = new DateTimeOffset(2026, 9, 14, 9, 30, 0, TimeSpan.FromHours(2));

        var ok = Booking.TryCreate("alpha", start, end, "Ada", "Standup", out _, out var violations);

        Assert.False(ok);
        Assert.Contains(BookingRuleViolation.NotUtc, violations);
    }
}
