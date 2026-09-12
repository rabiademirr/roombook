using RoomBook.Application;
using RoomBook.Domain;

namespace RoomBook.Application.Tests;

internal sealed class FakeRoomRepository : IRoomRepository
{
    private readonly List<Room> _rooms;

    public FakeRoomRepository(params Room[] rooms) => _rooms = rooms.ToList();

    public Room? GetById(string id) => _rooms.FirstOrDefault(r => r.Id == id);
}

internal sealed class FakeBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings;

    public FakeBookingRepository(params Booking[] seed) => _bookings = seed.ToList();

    public IReadOnlyList<Booking> GetByRoomId(string roomId) =>
        _bookings.Where(b => b.RoomId == roomId).ToList();

    public void Add(Booking booking) => _bookings.Add(booking);
}
