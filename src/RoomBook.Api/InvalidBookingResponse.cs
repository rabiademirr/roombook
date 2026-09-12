namespace RoomBook.Api;

public sealed record InvalidBookingResponse(IReadOnlyList<string> Violations);
