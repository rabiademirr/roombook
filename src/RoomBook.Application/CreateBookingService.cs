using RoomBook.Domain;

namespace RoomBook.Application;

public sealed class CreateBookingService
{
    private readonly IRoomRepository _rooms;
    private readonly IBookingRepository _bookings;

    public CreateBookingService(IRoomRepository rooms, IBookingRepository bookings)
    {
        _rooms = rooms;
        _bookings = bookings;
    }

    public CreateBookingResult CreateBooking(string roomId, DateTimeOffset start, DateTimeOffset end, string organizer, string title)
    {
        var room = _rooms.GetById(roomId);
        if (room is null)
            return CreateBookingResult.RoomNotFound();

        var isValid = Booking.TryCreate(roomId, start, end, organizer, title, out var booking, out var violations);
        if (!isValid)
            return CreateBookingResult.Invalid(violations);

        var existing = _bookings.GetByRoomId(roomId);
        var conflicts = existing
            .Where(b => BookingOverlap.Overlaps(b.TimeRange, booking!.TimeRange))
            .ToList();

        if (conflicts.Count > 0)
        {
            var suggestions = FreeSlotFinder.FindNearest(
                existing.Select(b => b.TimeRange).ToList(),
                start,
                end - start);
            return CreateBookingResult.Conflict(conflicts, suggestions);
        }

        _bookings.Add(booking!);
        return CreateBookingResult.Created(booking!);
    }
}
