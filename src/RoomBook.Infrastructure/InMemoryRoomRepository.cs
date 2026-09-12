using RoomBook.Application;
using RoomBook.Domain;

namespace RoomBook.Infrastructure;

public sealed class InMemoryRoomRepository : IRoomRepository
{
    private static readonly IReadOnlyList<Room> SeedRooms = new[]
    {
        new Room("alpha", "Alpha"),
        new Room("beta", "Beta"),
        new Room("gamma", "Gamma"),
    };

    public Room? GetById(string id) => SeedRooms.FirstOrDefault(r => r.Id == id);
}
