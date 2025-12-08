using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingManagmint.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        public IndexModel(AppDbContext db) => _db = db;

        public int TotalEvents { get; private set; }
        public int UpcomingThisWeek { get; private set; }

        public void OnGet()
        {
            var now = DateTime.UtcNow;
            TotalEvents = _db.Events.Count();
            UpcomingThisWeek = _db.Events.Count(e => e.DateTime >= now && e.DateTime <= now.AddDays(7));
        }
    }
}

