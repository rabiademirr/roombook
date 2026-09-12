namespace RoomBook.Api;

public sealed record ConflictResponseBody(
    IReadOnlyList<ConflictingBookingDto> Conflicts,
    IReadOnlyList<FreeSlotDto> Suggestions);
