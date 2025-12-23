using System.ComponentModel.DataAnnotations;
using BookingManagmint.Models;

namespace BookingManagmint.ViewModels
{
    public class TicketViewModel
    {
        public Guid TicketId { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public TicketStatus Status { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; }
        public string? SeatNumber { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDateTime { get; set; }
        public string EventVenue { get; set; } = string.Empty;
        public decimal EventPrice { get; set; }
    }

    public class BookTicketViewModel
    {
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDateTime { get; set; }
        public string EventVenue { get; set; } = string.Empty;
        public decimal EventPrice { get; set; }
        public int RemainingSeats { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
        public int Quantity { get; set; } = 1;
    }
}


