using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BookingManagmint.Data;
using BookingManagmint.Models;
using BookingManagmint.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingManagmint.Controllers
{
    public class OrganizerController : Controller
    {
        private readonly AppDbContext _context;

        public OrganizerController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdText, out var userId) ? userId : null;
        }

        private bool IsOrganizer()
        {
            var roleText = HttpContext.Session.GetString("UserRole");
            return roleText == "1" || roleText == "2"; // Organizer or Admin
        }

        // GET: Organizer - List organizer's events with search
        public async Task<IActionResult> Index(string? searchQuery, string? category)
        {
            if (!IsOrganizer())
            {
                return Redirect("/Auth/Login");
            }

            var query = _context.Events.AsQueryable();

            // Search functionality
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(e => 
                    e.Title.Contains(searchQuery) || 
                    e.Description.Contains(searchQuery) || 
                    e.Venue.Contains(searchQuery) ||
                    e.Category.Contains(searchQuery));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(e => e.Category == category);
            }

            var events = await query
                .OrderBy(e => e.DateTime)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    DateTime = e.DateTime,
                    Venue = e.Venue,
                    Capacity = e.Capacity,
                    RemainingSeats = e.RemainingSeats,
                    Price = e.Price,
                    Category = e.Category
                })
                .ToListAsync();

            var viewModel = new EventSearchViewModel
            {
                SearchQuery = searchQuery,
                Category = category,
                Events = events
            };

            ViewBag.Categories = await _context.Events
                .Select(e => e.Category)
                .Distinct()
                .ToListAsync();

            return View(viewModel);
        }

        // GET: Organizer/Create
        public IActionResult Create()
        {
            if (!IsOrganizer())
            {
                return Redirect("/Auth/Login");
            }

            ViewBag.Categories = new List<string> { "General", "Music", "Sports", "Technology", "Business", "Arts", "Education" };
            return View(new EventViewModel { DateTime = DateTime.UtcNow.AddDays(7) });
        }

        // POST: Organizer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!IsOrganizer())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new List<string> { "General", "Music", "Sports", "Technology", "Business", "Arts", "Education" };
                return View(model);
            }

            try
            {
                var eventEntity = new Event(
                    model.Title,
                    model.Description,
                    model.DateTime,
                    model.Venue,
                    model.Capacity,
                    model.Price,
                    model.Category
                );

                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating event: {ex.Message}");
                ViewBag.Categories = new List<string> { "General", "Music", "Sports", "Technology", "Business", "Arts", "Education" };
                return View(model);
            }
        }

        // GET: Organizer/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!IsOrganizer())
            {
                return Redirect("/Auth/Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var viewModel = new EventViewModel
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                DateTime = eventEntity.DateTime,
                Venue = eventEntity.Venue,
                Capacity = eventEntity.Capacity,
                RemainingSeats = eventEntity.RemainingSeats,
                Price = eventEntity.Price,
                Category = eventEntity.Category
            };

            ViewBag.Categories = new List<string> { "General", "Music", "Sports", "Technology", "Business", "Arts", "Education" };
            return View(viewModel);
        }

        // POST: Organizer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EventViewModel model)
        {
            if (!IsOrganizer())
            {
                return Redirect("/Auth/Login");
            }

            if (id != model.EventId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new List<string> { "General", "Music", "Sports", "Technology", "Business", "Arts", "Education" };
                return View(model);
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            try
            {
                // Calculate new remaining seats if capacity changed
                var capacityDiff = model.Capacity - eventEntity.Capacity;
                var newRemainingSeats = eventEntity.RemainingSeats + capacityDiff;
                if (newRemainingSeats < 0) newRemainingSeats = 0;
                if (newRemainingSeats > model.Capacity) newRemainingSeats = model.Capacity;

                eventEntity.Title = model.Title;
                eventEntity.Description = model.Description;
                eventEntity.DateTime = model.DateTime;
                eventEntity.Venue = model.Venue;
                eventEntity.Capacity = model.Capacity;
                eventEntity.RemainingSeats = newRemainingSeats;
                eventEntity.Price = model.Price;
                eventEntity.Category = model.Category;

                _context.Events.Update(eventEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EventExists(model.EventId))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating event: {ex.Message}");
                ViewBag.Categories = new List<string> { "General", "Music", "Sports", "Technology", "Business", "Arts", "Education" };
                return View(model);
            }
        }

        // GET: Organizer/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!IsOrganizer())
            {
                return Redirect("/Auth/Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var viewModel = new EventViewModel
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                DateTime = eventEntity.DateTime,
                Venue = eventEntity.Venue,
                Capacity = eventEntity.Capacity,
                RemainingSeats = eventEntity.RemainingSeats,
                Price = eventEntity.Price,
                Category = eventEntity.Category
            };

            // Check for existing tickets
            var ticketCount = await _context.Tickets
                .CountAsync(t => t.EventId == id);

            ViewBag.TicketCount = ticketCount;

            return View(viewModel);
        }

        // POST: Organizer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (!IsOrganizer())
            {
                return Redirect("/Auth/Login");
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            try
            {
                // Check for tickets - if there are tickets, we might want to prevent deletion
                // or handle it differently. For now, we'll allow deletion (cascade will handle tickets)
                _context.Events.Remove(eventEntity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Event deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting event: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task<bool> EventExists(Guid id)
        {
            return await _context.Events.AnyAsync(e => e.EventId == id);
        }
    }
}


