namespace RoomBook.Api;

public sealed record InvalidBookingResponse(IReadOnlyList<string> Violations);

public sealed record RoomNotFoundResponse(string Message);
