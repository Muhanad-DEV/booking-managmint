// Enumeration types for the event booking system
// Documented: These enums are used in models, page logic, and data constraints

namespace BookingManagmint.Models
{
    /// <summary>
    /// Represents the possible statuses for a ticket.
    /// </summary>
    public enum TicketStatus
    {
        Reserved, // Ticket reserved but not yet paid
        Paid,     // Ticket purchased
        Cancelled,// Booking cancelled
        CheckedIn // Attendee checked in at event
    }

    /// <summary>
    /// System roles for users
    /// </summary>
    public enum UserRole
    {
        Attendee,   // Can browse/book/cancel
        Organizer,  // Can create/edit events
        Admin       // Full admin privileges
    }
}
