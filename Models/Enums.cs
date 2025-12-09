namespace BookingManagmint.Models
{
    public enum TicketStatus
    {
        Reserved,
        Paid,
        Cancelled,
        CheckedIn
    }

    public enum UserRole
    {
        Attendee,
        Organizer,
        Admin
    }
}
