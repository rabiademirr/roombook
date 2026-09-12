namespace RoomBook.Api;

public sealed record ConflictingBookingDto(DateTimeOffset Start, DateTimeOffset End, string Organizer);

public sealed record FreeSlotDto(DateTimeOffset Start, DateTimeOffset End);

public sealed record ConflictResponseBody(
    IReadOnlyList<ConflictingBookingDto> Conflicts,
    IReadOnlyList<FreeSlotDto> Suggestions);
