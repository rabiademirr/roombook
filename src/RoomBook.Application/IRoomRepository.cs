using RoomBook.Domain;

namespace RoomBook.Application;

public interface IRoomRepository
{
    Room? GetById(string id);
}
