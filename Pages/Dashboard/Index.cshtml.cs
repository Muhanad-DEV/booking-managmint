using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Services;

namespace BookingManagmint.Pages.Dashboard
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly BookingService _booking = new BookingService();

        public IActionResult OnGetTickets()
        {
            // Demo: first attendee
            var user = InMemoryStore.Users.FirstOrDefault(u => u.Role == Models.UserRole.Attendee);
            if (user == null) return new JsonResult(Array.Empty<object>());
            var items = _booking.ListUserTickets(user.UserId).Select(t => new {
                id = t.TicketId,
                eventId = t.EventId,
                status = t.Status.ToString(),
                purchasedAt = t.PurchasedAt
            });
            return new JsonResult(items);
        }

        public IActionResult OnPostCancel(Guid ticketId)
        {
            var ok = _booking.CancelTicket(ticketId);
            if (!ok) return BadRequest();
            return new JsonResult(new { ok = true });
        }
    }
}

