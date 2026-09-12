using RoomBook.Application;
using RoomBook.Domain;

namespace RoomBook.Application.Tests;

public class CreateBookingServiceTests
{
    private static DateTimeOffset Utc(int hour, int minute) =>
        new(2026, 9, 14, hour, minute, 0, TimeSpan.Zero);

    private static Booking ExistingBooking(string roomId, int startHour, int startMinute, int endHour, int endMinute)
    {
        Booking.TryCreate(roomId, Utc(startHour, startMinute), Utc(endHour, endMinute), "Existing Organizer", "Existing Title", out var booking, out _);
        return booking!;
    }

    [Fact]
    public void CreateBooking_ValidRequest_PersistsAndReturnsCreated()
    {
        var rooms = new FakeRoomRepository(new Room("alpha", "Alpha"));
        var bookings = new FakeBookingRepository();
        var service = new CreateBookingService(rooms, bookings);

        var result = service.CreateBooking("alpha", Utc(9, 0), Utc(9, 30), "Ada", "Standup");

        Assert.Equal(CreateBookingOutcomeKind.Created, result.Kind);
        Assert.NotNull(result.Booking);
        Assert.Single(bookings.GetByRoomId("alpha"));
    }

    [Fact]
    public void CreateBooking_UnknownRoom_ReturnsRoomNotFoundWithoutConflictsOrSuggestions()
    {
        var rooms = new FakeRoomRepository();
        var bookings = new FakeBookingRepository();
        var service = new CreateBookingService(rooms, bookings);

        var result = service.CreateBooking("unknown", Utc(9, 0), Utc(9, 30), "Ada", "Standup");

        Assert.Equal(CreateBookingOutcomeKind.RoomNotFound, result.Kind);
        Assert.Empty(result.ConflictingBookings);
        Assert.Empty(result.Suggestions);
    }

    [Fact]
    public void CreateBooking_SpansTwoExistingBookings_ListsBothAsConflicts()
    {
        var first = ExistingBooking("alpha", 9, 0, 9, 30);
        var second = ExistingBooking("alpha", 10, 0, 10, 30);
        var rooms = new FakeRoomRepository(new Room("alpha", "Alpha"));
        var bookings = new FakeBookingRepository(first, second);
        var service = new CreateBookingService(rooms, bookings);

        var result = service.CreateBooking("alpha", Utc(9, 15), Utc(10, 15), "Ada", "Standup");

        Assert.Equal(CreateBookingOutcomeKind.Conflict, result.Kind);
        Assert.Equal(2, result.ConflictingBookings.Count);
        Assert.Contains(result.ConflictingBookings, b => b.Id == first.Id);
        Assert.Contains(result.ConflictingBookings, b => b.Id == second.Id);
    }

    [Fact]
    public void CreateBooking_BackToBackRequest_Succeeds()
    {
        var existing = ExistingBooking("alpha", 9, 0, 10, 0);
        var rooms = new FakeRoomRepository(new Room("alpha", "Alpha"));
        var bookings = new FakeBookingRepository(existing);
        var service = new CreateBookingService(rooms, bookings);

        var result = service.CreateBooking("alpha", Utc(10, 0), Utc(10, 30), "Ada", "Standup");

        Assert.Equal(CreateBookingOutcomeKind.Created, result.Kind);
    }

    [Fact]
    public void CreateBooking_InvalidRequest_ReturnsInvalidAndDoesNotPersist()
    {
        var rooms = new FakeRoomRepository(new Room("alpha", "Alpha"));
        var bookings = new FakeBookingRepository();
        var service = new CreateBookingService(rooms, bookings);

        var result = service.CreateBooking("alpha", Utc(9, 0), Utc(9, 0), "Ada", "Standup");

        Assert.Equal(CreateBookingOutcomeKind.Invalid, result.Kind);
        Assert.Contains(BookingRuleViolation.InvalidDuration, result.Violations);
        Assert.Empty(bookings.GetByRoomId("alpha"));
    }
}
