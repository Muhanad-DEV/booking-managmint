using System;

namespace BookingManagmint.Models
{
    /// <summary>
    /// Represents a booked ticket by a user for an event.
    /// </summary>
    public class Ticket
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; } // FK to User
        public Guid EventId { get; set; } // FK to Event
        public TicketStatus Status { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; }
        public string? SeatNumber { get; set; }

        public Ticket() { }
        public Ticket(Guid userId, Guid eventId, string qrCode, string? seatNumber=null)
        {
            TicketId = Guid.NewGuid();
            UserId = userId;
            EventId = eventId;
            Status = TicketStatus.Reserved;
            QRCode = qrCode;
            PurchasedAt = DateTime.UtcNow;
            SeatNumber = seatNumber;
        }

        /// <summary>
        /// Mark ticket as paid after purchase.
        /// </summary>
        public void MarkPaid() => Status = TicketStatus.Paid;
        /// <summary>
        /// Mark ticket as cancelled (release seat).
        /// </summary>
        public void Cancel() => Status = TicketStatus.Cancelled;
        /// <summary>
        /// Mark ticket as checked in at venue.
        /// </summary>
        public void MarkCheckedIn() => Status = TicketStatus.CheckedIn;
    }
}
