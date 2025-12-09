using System;

namespace BookingManagmint.Models
{
    public class Ticket
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
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

        public void MarkPaid() => Status = TicketStatus.Paid;
        public void Cancel() => Status = TicketStatus.Cancelled;
        public void MarkCheckedIn() => Status = TicketStatus.CheckedIn;
    }
}
