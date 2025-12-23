using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BookingManagmint.Data;
using BookingManagmint.Models;

namespace BookingManagmint.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsApiController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdText, out var userId) ? userId : null;
        }

        // GET: api/TicketsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTickets()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId.Value)
                .Join(_context.Events,
                    ticket => ticket.EventId,
                    eventEntity => eventEntity.EventId,
                    (ticket, eventEntity) => new
                    {
                        ticketId = ticket.TicketId,
                        userId = ticket.UserId,
                        eventId = ticket.EventId,
                        status = ticket.Status.ToString(),
                        qrCode = ticket.QRCode,
                        purchasedAt = ticket.PurchasedAt,
                        seatNumber = ticket.SeatNumber,
                        eventTitle = eventEntity.Title,
                        eventDateTime = eventEntity.DateTime,
                        eventVenue = eventEntity.Venue,
                        eventPrice = eventEntity.Price
                    })
                .OrderByDescending(t => t.purchasedAt)
                .ToListAsync();

            return Ok(tickets);
        }

        // GET: api/TicketsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTicket(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            var ticket = await _context.Tickets
                .Where(t => t.TicketId == id && t.UserId == userId.Value)
                .Join(_context.Events,
                    ticket => ticket.EventId,
                    eventEntity => eventEntity.EventId,
                    (ticket, eventEntity) => new
                    {
                        ticketId = ticket.TicketId,
                        userId = ticket.UserId,
                        eventId = ticket.EventId,
                        status = ticket.Status.ToString(),
                        qrCode = ticket.QRCode,
                        purchasedAt = ticket.PurchasedAt,
                        seatNumber = ticket.SeatNumber,
                        eventTitle = eventEntity.Title,
                        eventDateTime = eventEntity.DateTime,
                        eventVenue = eventEntity.Venue,
                        eventPrice = eventEntity.Price
                    })
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound(new { error = "Ticket not found" });
            }

            return Ok(ticket);
        }

        // POST: api/TicketsApi
        [HttpPost]
        public async Task<ActionResult<object>> BookTicket([FromBody] BookTicketRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            if (request.Quantity <= 0 || request.Quantity > 10)
            {
                return BadRequest(new { error = "Quantity must be between 1 and 10" });
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == request.EventId);

            if (eventEntity == null)
            {
                return NotFound(new { error = "Event not found" });
            }

            if (!eventEntity.CanBook(request.Quantity))
            {
                return BadRequest(new { error = "Not enough seats available" });
            }

            try
            {
                // Reserve seats
                eventEntity.ReserveSeats(request.Quantity);
                _context.Events.Update(eventEntity);

                // Create tickets
                var tickets = new List<object>();
                for (int i = 0; i < request.Quantity; i++)
                {
                    var ticketId = Guid.NewGuid();
                    var qrCode = $"QR-{ticketId.ToString()[..8].ToUpper()}";
                    
                    var ticket = new Ticket(userId.Value, eventEntity.EventId, qrCode);
                    ticket.MarkPaid(); // Mark as paid for simplicity
                    
                    _context.Tickets.Add(ticket);

                    tickets.Add(new
                    {
                        ticketId = ticket.TicketId,
                        qrCode = ticket.QRCode,
                        status = ticket.Status.ToString(),
                        purchasedAt = ticket.PurchasedAt
                    });
                }

                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTicket), new { id = tickets.FirstOrDefault() }, new
                {
                    message = $"Successfully booked {request.Quantity} ticket(s)",
                    tickets = tickets
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error booking ticket: {ex.Message}" });
            }
        }

        // PUT: api/TicketsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == id && t.UserId == userId.Value);

            if (ticket == null)
            {
                return NotFound(new { error = "Ticket not found" });
            }

            try
            {
                // Update ticket status
                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (request.Status.ToLower() == "cancelled" && ticket.Status != TicketStatus.Cancelled)
                    {
                        var eventEntity = await _context.Events
                            .FirstOrDefaultAsync(e => e.EventId == ticket.EventId);

                        if (eventEntity != null)
                        {
                            eventEntity.ReleaseSeats(1);
                            _context.Events.Update(eventEntity);
                        }

                        ticket.Cancel();
                    }
                    else if (request.Status.ToLower() == "paid" && ticket.Status == TicketStatus.Reserved)
                    {
                        ticket.MarkPaid();
                    }
                    else if (request.Status.ToLower() == "checkedin" && ticket.Status == TicketStatus.Paid)
                    {
                        ticket.MarkCheckedIn();
                    }
                }

                if (!string.IsNullOrEmpty(request.SeatNumber))
                {
                    ticket.SeatNumber = request.SeatNumber;
                }

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    ticketId = ticket.TicketId,
                    status = ticket.Status.ToString(),
                    qrCode = ticket.QRCode,
                    seatNumber = ticket.SeatNumber
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error updating ticket: {ex.Message}" });
            }
        }

        // DELETE: api/TicketsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketId == id && t.UserId == userId.Value);

            if (ticket == null)
            {
                return NotFound(new { error = "Ticket not found" });
            }

            try
            {
                // Release seat if ticket is not already cancelled
                if (ticket.Status != TicketStatus.Cancelled)
                {
                    var eventEntity = await _context.Events
                        .FirstOrDefaultAsync(e => e.EventId == ticket.EventId);

                    if (eventEntity != null)
                    {
                        eventEntity.ReleaseSeats(1);
                        _context.Events.Update(eventEntity);
                    }
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Ticket deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error deleting ticket: {ex.Message}" });
            }
        }
    }

    public class BookTicketRequest
    {
        public Guid EventId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateTicketRequest
    {
        public string? Status { get; set; }
        public string? SeatNumber { get; set; }
    }
}


