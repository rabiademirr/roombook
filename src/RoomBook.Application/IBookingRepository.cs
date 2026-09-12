using RoomBook.Domain;

namespace RoomBook.Application;

public interface IBookingRepository
{
    IReadOnlyList<Booking> GetByRoomId(string roomId);

    void Add(Booking booking);
}
