using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using BookingManagmint.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace BookingManagmint.Pages.Organizer
{
    public class EventsModel : PageModel
    {
        private readonly AppDbContext _db;
        public EventsModel(AppDbContext db) => _db = db;

        public string? Message { get; private set; }
        public bool IsSuccess { get; private set; }

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

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Message = "Please fill in all required fields correctly.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                var ev = new Event(ModelInput.Title, ModelInput.Description ?? string.Empty, ModelInput.DateTime, ModelInput.Venue, ModelInput.Capacity, ModelInput.Price, ModelInput.Category);
                _db.Events.Add(ev);
                _db.SaveChanges();

                Message = "Event created successfully!";
                IsSuccess = true;
                ModelInput = new Input();
                return Page();
            }
            catch (Exception ex)
            {
                Message = $"Error creating event: {ex.Message}";
                IsSuccess = false;
                return Page();
            }
        }

        public async Task<IActionResult> OnGetListAsync()
        {
            var userName = HttpContext.Session.GetString("UserName") ?? "Organizer";
            var events = await _db.Events
                .OrderBy(e => e.DateTime)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.DateTime,
                    e.Venue,
                    e.RemainingSeats,
                    e.Category
                })
                .ToListAsync();
            return new JsonResult(new { organizer = userName, events });
        }

        public IActionResult OnPostCreate()
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return new JsonResult(new { error = "Validation failed", details = ModelState });
            }

            var ev = new Event(ModelInput.Title, ModelInput.Description ?? string.Empty, ModelInput.DateTime, ModelInput.Venue, ModelInput.Capacity, ModelInput.Price, ModelInput.Category);
            _db.Events.Add(ev);
            _db.SaveChanges();

            return new JsonResult(new { id = ev.EventId });
        }

        public IActionResult OnPostDelete(Guid id)
        {
            var ev = _db.Events.FirstOrDefault(e => e.EventId == id);
            if (ev == null) return NotFound();
            _db.Events.Remove(ev);
            _db.SaveChanges();
            return new JsonResult(new { ok = true });
        }

        public IActionResult OnPostUpdate(Guid id, decimal price)
        {
            var ev = _db.Events.FirstOrDefault(e => e.EventId == id);
            if (ev == null) return NotFound();
            ev.Price = price;
            _db.SaveChanges();
            return new JsonResult(new { ok = true, newPrice = price });
        }
    }
}

