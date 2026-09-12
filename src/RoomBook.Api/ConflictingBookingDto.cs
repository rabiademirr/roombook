namespace RoomBook.Api;

public sealed record ConflictingBookingDto(DateTimeOffset Start, DateTimeOffset End, string Organizer);
