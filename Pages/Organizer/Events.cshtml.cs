using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Services;
using System.ComponentModel.DataAnnotations;

namespace BookingManagmint.Pages.Organizer
{
    [IgnoreAntiforgeryToken]
    public class EventsModel : PageModel
    {
        private readonly BookingService _booking = new BookingService();

        public class Input
        {
            [Required]
            [StringLength(120)]
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            [Required]
            public DateTime DateTime { get; set; }
            [Required]
            [StringLength(200)]
            public string Venue { get; set; } = string.Empty;
            [Range(0, int.MaxValue)]
            public int Capacity { get; set; }
            [Range(0, double.MaxValue)]
            public decimal Price { get; set; }
            public string Category { get; set; } = "General";
        }

        [BindProperty]
        public Input ModelInput { get; set; } = new Input();

        public IActionResult OnPostCreate()
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return new JsonResult(new { error = "Validation failed", details = ModelState });
            }

            var ev = _booking.CreateEvent(ModelInput.Title, ModelInput.Description ?? string.Empty, ModelInput.DateTime, ModelInput.Venue, ModelInput.Capacity, ModelInput.Price, ModelInput.Category);

            return new JsonResult(new { id = ev.EventId });
        }
    }
}

