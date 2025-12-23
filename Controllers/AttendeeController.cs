using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BookingManagmint.Data;
using BookingManagmint.Models;
using BookingManagmint.ViewModels;

namespace BookingManagmint.Controllers
{
    public class AttendeeController : Controller
    {
        private readonly AppDbContext _context;

        public AttendeeController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdText, out var userId) ? userId : null;
        }

        // GET: Attendee - Search/browse events
        public async Task<IActionResult> Index(string? searchQuery, string? category)
        {
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
                .Where(e => e.DateTime >= DateTime.UtcNow)
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

        // GET: Attendee/BookTicket/5
        public async Task<IActionResult> BookTicket(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            var viewModel = new BookTicketViewModel
            {
                EventId = eventEntity.EventId,
                EventTitle = eventEntity.Title,
                EventDateTime = eventEntity.DateTime,
                EventVenue = eventEntity.Venue,
                EventPrice = eventEntity.Price,
                RemainingSeats = eventEntity.RemainingSeats
            };

            return View(viewModel);
        }

        // POST: Attendee/BookTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTicket(BookTicketViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == model.EventId);
                
                if (eventEntity != null)
                {
                    model.EventTitle = eventEntity.Title;
                    model.EventDateTime = eventEntity.DateTime;
                    model.EventVenue = eventEntity.Venue;
                    model.EventPrice = eventEntity.Price;
                    model.RemainingSeats = eventEntity.RemainingSeats;
                }
                return View(model);
            }

            var eventToBook = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == model.EventId);

            if (eventToBook == null)
            {
                ModelState.AddModelError("", "Event not found.");
                return View(model);
            }

            if (!eventToBook.CanBook(model.Quantity))
            {
                ModelState.AddModelError("Quantity", "Not enough seats available.");
                model.EventTitle = eventToBook.Title;
                model.EventDateTime = eventToBook.DateTime;
                model.EventVenue = eventToBook.Venue;
                model.EventPrice = eventToBook.Price;
                model.RemainingSeats = eventToBook.RemainingSeats;
                return View(model);
            }

            try
            {
                // Reserve seats
                eventToBook.ReserveSeats(model.Quantity);
                _context.Events.Update(eventToBook);

                // Create tickets
                for (int i = 0; i < model.Quantity; i++)
                {
                    var ticketId = Guid.NewGuid();
                    var qrCode = $"QR-{ticketId.ToString()[..8].ToUpper()}";
                    
                    var ticket = new Ticket(userId.Value, eventToBook.EventId, qrCode);
                    ticket.MarkPaid(); // Mark as paid for simplicity
                    
                    _context.Tickets.Add(ticket);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully booked {model.Quantity} ticket(s)!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error booking ticket: {ex.Message}");
                model.EventTitle = eventToBook.Title;
                model.EventDateTime = eventToBook.DateTime;
                model.EventVenue = eventToBook.Venue;
                model.EventPrice = eventToBook.Price;
                model.RemainingSeats = eventToBook.RemainingSeats;
                return View(model);
            }
        }

        // GET: Attendee/CancelTicket/5
        public async Task<IActionResult> CancelTicket(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            var ticket = await _context.Tickets
                .Where(t => t.TicketId == id && t.UserId == userId.Value)
                .Select(t => new TicketViewModel
                {
                    TicketId = t.TicketId,
                    UserId = t.UserId,
                    EventId = t.EventId,
                    Status = t.Status,
                    QRCode = t.QRCode,
                    PurchasedAt = t.PurchasedAt,
                    SeatNumber = t.SeatNumber
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound();
            }

            // Get event details
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == ticket.EventId);

            if (eventEntity != null)
            {
                ticket.EventTitle = eventEntity.Title;
                ticket.EventDateTime = eventEntity.DateTime;
                ticket.EventVenue = eventEntity.Venue;
                ticket.EventPrice = eventEntity.Price;
            }

            return View(ticket);
        }

        // POST: Attendee/CancelTicket/5
        [HttpPost, ActionName("CancelTicket")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTicketConfirmed(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == id && t.UserId == userId.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Status == TicketStatus.Cancelled)
            {
                TempData["ErrorMessage"] = "Ticket is already cancelled.";
                return RedirectToAction(nameof(MyTickets));
            }

            try
            {
                var eventEntity = await _context.Events
                    .FirstOrDefaultAsync(e => e.EventId == ticket.EventId);

                if (eventEntity != null)
                {
                    // Release seat
                    eventEntity.ReleaseSeats(1);
                    _context.Events.Update(eventEntity);
                }

                // Cancel ticket
                ticket.Cancel();
                _context.Tickets.Update(ticket);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ticket cancelled successfully.";
                return RedirectToAction(nameof(MyTickets));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling ticket: {ex.Message}";
                return RedirectToAction(nameof(MyTickets));
            }
        }

        // GET: Attendee/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId.Value)
                .Join(_context.Events,
                    ticket => ticket.EventId,
                    eventEntity => eventEntity.EventId,
                    (ticket, eventEntity) => new TicketViewModel
                    {
                        TicketId = ticket.TicketId,
                        UserId = ticket.UserId,
                        EventId = ticket.EventId,
                        Status = ticket.Status,
                        QRCode = ticket.QRCode,
                        PurchasedAt = ticket.PurchasedAt,
                        SeatNumber = ticket.SeatNumber,
                        EventTitle = eventEntity.Title,
                        EventDateTime = eventEntity.DateTime,
                        EventVenue = eventEntity.Venue,
                        EventPrice = eventEntity.Price
                    })
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();

            return View(tickets);
        }

        // GET: Attendee/UpdateProfile
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserProfileViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };

            return View(viewModel);
        }

        // POST: Attendee/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Redirect("/Auth/Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return NotFound();
            }

            // Check if email is already taken by another user
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.UserId != userId.Value);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            try
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Update session
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);

                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(UpdateProfile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating profile: {ex.Message}");
                return View(model);
            }
        }
    }
}

