using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using Microsoft.Data.SqlClient;

namespace BookingManagmint.Pages.Events
{
    public class IndexModel : PageModel
    {
        private readonly DbConnectionFactory _factory;
        public IndexModel(DbConnectionFactory factory) => _factory = factory;

        public List<EventItem> Events { get; private set; } = new();
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
            SearchQuery = q;
            var rows = new List<EventItem>();
            using var conn = _factory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TOP 100 EventId, Title, DateTime, Venue, RemainingSeats, Category
                                FROM Events
                                WHERE (@q IS NULL OR Title LIKE '%' + @q + '%' OR Venue LIKE '%' + @q + '%' OR Category LIKE '%' + @q + '%')
                                ORDER BY DateTime";
            cmd.Parameters.Add(new SqlParameter("@q", (object?)q ?? DBNull.Value));
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new EventItem
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    DateTime = reader.GetDateTime(2),
                    Venue = reader.GetString(3),
                    Remaining = reader.GetInt32(4),
                    Category = reader.GetString(5)
                });
            }
            Events = rows;
        }

        public IActionResult OnGetList(string? q)
        {
            var rows = new List<object>();
            using var conn = _factory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TOP 100 EventId, Title, DateTime, Venue, RemainingSeats, Category
                                FROM Events
                                WHERE (@q IS NULL OR Title LIKE '%' + @q + '%')
                                ORDER BY DateTime";
            cmd.Parameters.Add(new SqlParameter("@q", (object?)q ?? DBNull.Value));
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new
                {
                    id = reader.GetGuid(0),
                    title = reader.GetString(1),
                    dateTime = reader.GetDateTime(2),
                    venue = reader.GetString(3),
                    remaining = reader.GetInt32(4),
                    category = reader.GetString(5)
                });
            }
            return new JsonResult(rows);
        }
    }
}

