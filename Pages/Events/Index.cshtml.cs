using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Services;

namespace BookingManagmint.Pages.Events
{
    public class IndexModel : PageModel
    {
        private readonly BookingService _booking;
        public IndexModel()
        {
            _booking = new BookingService();
        }

        public IActionResult OnGetList(string? q)
        {
            var items = _booking.ListEvents(q).Select(e => new {
                id = e.EventId,
                title = e.Title,
                dateTime = e.DateTime,
                venue = e.Venue,
                remaining = e.RemainingSeats,
                category = e.Category
            });
            return new JsonResult(items);
        }
    }
}

