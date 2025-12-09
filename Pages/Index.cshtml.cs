using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using Microsoft.EntityFrameworkCore;
using BookingManagmint.Models;

namespace BookingManagmint.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        public IndexModel(AppDbContext db) => _db = db;

        public int TotalEvents { get; private set; }
        public int UpcomingThisWeek { get; private set; }
        public List<EventItem> FeaturedEvents { get; private set; } = new();
        public List<EventItem> AllEvents { get; private set; } = new();
        public string? SearchQuery { get; private set; }

        public class EventItem
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime DateTime { get; set; }
            public string Venue { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public int Remaining { get; set; }
        }

        public void OnGet(string? q = null)
        {
            var now = DateTime.UtcNow;
            TotalEvents = _db.Events.Count();
            UpcomingThisWeek = _db.Events.Count(e => e.DateTime >= now && e.DateTime <= now.AddDays(7));
            SearchQuery = q;

            var query = _db.Events.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchTerm = q.ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(searchTerm) || 
                                         e.Venue.ToLower().Contains(searchTerm) || 
                                         e.Category.ToLower().Contains(searchTerm));
            }

            var events = query
                .OrderBy(e => e.DateTime)
                .Select(e => new EventItem
                {
                    Id = e.EventId,
                    Title = e.Title,
                    DateTime = e.DateTime,
                    Venue = e.Venue,
                    Category = e.Category,
                    Remaining = e.RemainingSeats
                })
                .ToList();

            FeaturedEvents = events.Where(e => e.DateTime >= now).Take(3).ToList();
            AllEvents = events;
        }
    }
}

