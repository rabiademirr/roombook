using RoomBook.Application;
using RoomBook.Domain;

namespace RoomBook.Infrastructure;

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = new();

    public IReadOnlyList<Booking> GetByRoomId(string roomId) =>
        _bookings.Where(b => b.RoomId == roomId).ToList();

    public void Add(Booking booking) => _bookings.Add(booking);
}
