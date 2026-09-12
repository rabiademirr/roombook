namespace RoomBook.Domain;

public static class BookingOverlap
{
    public static bool Overlaps(TimeRange a, TimeRange b) =>
        a.Start < b.End && b.Start < a.End;
}
