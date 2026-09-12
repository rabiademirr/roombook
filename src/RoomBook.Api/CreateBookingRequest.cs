namespace RoomBook.Api;

public sealed record CreateBookingRequest(
    string RoomId,
    DateTimeOffset Start,
    DateTimeOffset End,
    string Organizer,
    string Title);
