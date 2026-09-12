namespace RoomBook.Domain;

public enum BookingRuleViolation
{
    OutsideBusinessHours,
    InvalidDuration,
    NotAligned,
    NotUtc,
}
