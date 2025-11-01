using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Services;

namespace BookingManagmint.Pages.Events
{
    [IgnoreAntiforgeryToken]
    public class DetailsModel : PageModel
    {
        private readonly BookingService _booking;
        public DetailsModel() { _booking = new BookingService(); }

        public IActionResult OnGetDetails(Guid id)
        {
            var ev = _booking.GetEvent(id);
            if (ev == null) return NotFound();
            return new JsonResult(new {
                id = ev.EventId,
                title = ev.Title,
                description = ev.Description,
                dateTime = ev.DateTime,
                venue = ev.Venue,
                remaining = ev.RemainingSeats,
                capacity = ev.Capacity,
                price = ev.Price,
                category = ev.Category
            });
        }

        public IActionResult OnPostBook(Guid id, int quantity = 1)
        {
            // For prototype, pick the first attendee user
            var user = InMemoryStore.Users.FirstOrDefault(u => u.Role == Models.UserRole.Attendee);
            if (user == null) return BadRequest("No attendee user available");
            if (quantity <= 0) return BadRequest("Quantity must be at least 1");
            try
            {
                var ticket = _booking.BookTickets(user.UserId, id, quantity);
                if (ticket == null) return BadRequest("Booking failed");
                return new JsonResult(new { ticketId = ticket.TicketId, status = ticket.Status.ToString() });
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return new JsonResult(new { error = "Unexpected error while booking" });
            }
        }
    }
}

