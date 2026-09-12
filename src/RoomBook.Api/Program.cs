using RoomBook.Api;
using RoomBook.Application;
using RoomBook.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddSingleton<CreateBookingService>();

var app = builder.Build();

app.MapPost("/bookings", (CreateBookingRequest request, CreateBookingService service) =>
{
    var fieldViolations = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Organizer))
        fieldViolations.Add("MissingOrganizer");
    if (string.IsNullOrWhiteSpace(request.Title))
        fieldViolations.Add("MissingTitle");
    if (fieldViolations.Count > 0)
        return Results.BadRequest(new InvalidBookingResponse(fieldViolations));

    var result = service.CreateBooking(request.RoomId, request.Start, request.End, request.Organizer, request.Title);

    return result.Kind switch
    {
        CreateBookingOutcomeKind.Created => Results.Created(
            $"/bookings/{result.Booking!.Id}",
            new CreateBookingResponse(result.Booking.Id, result.Booking.RoomId, result.Booking.Start, result.Booking.End, result.Booking.Organizer, result.Booking.Title)),

        CreateBookingOutcomeKind.Invalid => Results.BadRequest(
            new InvalidBookingResponse(result.Violations.Select(v => v.ToString()).ToList())),

        CreateBookingOutcomeKind.RoomNotFound => Results.NotFound(
            new RoomNotFoundResponse($"Room '{request.RoomId}' does not exist.")),

        CreateBookingOutcomeKind.Conflict => Results.Conflict(
            new ConflictResponseBody(
                result.ConflictingBookings.Select(b => new ConflictingBookingDto(b.Start, b.End, b.Organizer)).ToList(),
                result.Suggestions.Select(s => new FreeSlotDto(s.Start, s.End)).ToList())),

        _ => Results.Problem("Unexpected outcome."),
    };
});

app.Run();

public partial class Program { }
