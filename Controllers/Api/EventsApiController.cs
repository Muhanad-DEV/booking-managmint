using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BookingManagmint.Data;
using BookingManagmint.Models;

namespace BookingManagmint.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventsApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EventsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEvents(
            [FromQuery] string? searchQuery,
            [FromQuery] string? category,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
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

            // Date range filter
            if (startDate.HasValue)
            {
                query = query.Where(e => e.DateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.DateTime <= endDate.Value);
            }

            var events = await query
                .OrderBy(e => e.DateTime)
                .Select(e => new
                {
                    eventId = e.EventId,
                    title = e.Title,
                    description = e.Description,
                    dateTime = e.DateTime,
                    venue = e.Venue,
                    capacity = e.Capacity,
                    remainingSeats = e.RemainingSeats,
                    price = e.Price,
                    category = e.Category
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/EventsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEvent(Guid id)
        {
            var eventEntity = await _context.Events
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    eventId = e.EventId,
                    title = e.Title,
                    description = e.Description,
                    dateTime = e.DateTime,
                    venue = e.Venue,
                    capacity = e.Capacity,
                    remainingSeats = e.RemainingSeats,
                    price = e.Price,
                    category = e.Category
                })
                .FirstOrDefaultAsync();

            if (eventEntity == null)
            {
                return NotFound(new { error = "Event not found" });
            }

            return Ok(eventEntity);
        }

        // POST: api/EventsApi
        [HttpPost]
        public async Task<ActionResult<object>> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user is organizer (optional - can be enhanced with authentication)
            var userIdText = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdText))
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            try
            {
                var eventEntity = new Event(
                    request.Title,
                    request.Description ?? string.Empty,
                    request.DateTime,
                    request.Venue,
                    request.Capacity,
                    request.Price,
                    request.Category
                );

                _context.Events.Add(eventEntity);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.EventId }, new
                {
                    eventId = eventEntity.EventId,
                    title = eventEntity.Title,
                    description = eventEntity.Description,
                    dateTime = eventEntity.DateTime,
                    venue = eventEntity.Venue,
                    capacity = eventEntity.Capacity,
                    remainingSeats = eventEntity.RemainingSeats,
                    price = eventEntity.Price,
                    category = eventEntity.Category
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error creating event: {ex.Message}" });
            }
        }

        // PUT: api/EventsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdText = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdText))
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound(new { error = "Event not found" });
            }

            try
            {
                // Calculate new remaining seats if capacity changed
                var capacityDiff = request.Capacity - eventEntity.Capacity;
                var newRemainingSeats = eventEntity.RemainingSeats + capacityDiff;
                if (newRemainingSeats < 0) newRemainingSeats = 0;
                if (newRemainingSeats > request.Capacity) newRemainingSeats = request.Capacity;

                eventEntity.Title = request.Title;
                eventEntity.Description = request.Description ?? string.Empty;
                eventEntity.DateTime = request.DateTime;
                eventEntity.Venue = request.Venue;
                eventEntity.Capacity = request.Capacity;
                eventEntity.RemainingSeats = newRemainingSeats;
                eventEntity.Price = request.Price;
                eventEntity.Category = request.Category;

                _context.Events.Update(eventEntity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    eventId = eventEntity.EventId,
                    title = eventEntity.Title,
                    description = eventEntity.Description,
                    dateTime = eventEntity.DateTime,
                    venue = eventEntity.Venue,
                    capacity = eventEntity.Capacity,
                    remainingSeats = eventEntity.RemainingSeats,
                    price = eventEntity.Price,
                    category = eventEntity.Category
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error updating event: {ex.Message}" });
            }
        }

        // DELETE: api/EventsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdText))
            {
                return Unauthorized(new { error = "Authentication required" });
            }

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound(new { error = "Event not found" });
            }

            try
            {
                _context.Events.Remove(eventEntity);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Event deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error deleting event: {ex.Message}" });
            }
        }
    }

    public class CreateEventRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateTime { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = "General";
    }

    public class UpdateEventRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateTime { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = "General";
    }
}


