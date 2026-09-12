namespace RoomBook.Api;

public sealed record CreateBookingResponse(
    Guid Id,
    string RoomId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string Organizer,
    string Title);
