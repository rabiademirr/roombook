using RoomBook.Domain;

namespace RoomBook.Application;

public enum CreateBookingOutcomeKind
{
    Created,
    Invalid,
    RoomNotFound,
    Conflict,
}

public sealed class CreateBookingResult
{
    public CreateBookingOutcomeKind Kind { get; }
    public Booking? Booking { get; }
    public IReadOnlyList<BookingRuleViolation> Violations { get; }
    public IReadOnlyList<Booking> ConflictingBookings { get; }
    public IReadOnlyList<TimeRange> Suggestions { get; }

    private CreateBookingResult(
        CreateBookingOutcomeKind kind,
        Booking? booking,
        IReadOnlyList<BookingRuleViolation> violations,
        IReadOnlyList<Booking> conflictingBookings,
        IReadOnlyList<TimeRange> suggestions)
    {
        Kind = kind;
        Booking = booking;
        Violations = violations;
        ConflictingBookings = conflictingBookings;
        Suggestions = suggestions;
    }

    public static CreateBookingResult Created(Booking booking) =>
        new(CreateBookingOutcomeKind.Created, booking, Array.Empty<BookingRuleViolation>(), Array.Empty<Booking>(), Array.Empty<TimeRange>());

    public static CreateBookingResult Invalid(IReadOnlyList<BookingRuleViolation> violations) =>
        new(CreateBookingOutcomeKind.Invalid, null, violations, Array.Empty<Booking>(), Array.Empty<TimeRange>());

    public static CreateBookingResult RoomNotFound() =>
        new(CreateBookingOutcomeKind.RoomNotFound, null, Array.Empty<BookingRuleViolation>(), Array.Empty<Booking>(), Array.Empty<TimeRange>());

    public static CreateBookingResult Conflict(IReadOnlyList<Booking> conflictingBookings, IReadOnlyList<TimeRange> suggestions) =>
        new(CreateBookingOutcomeKind.Conflict, null, Array.Empty<BookingRuleViolation>(), conflictingBookings, suggestions);
}
