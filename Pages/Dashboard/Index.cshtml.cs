using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using BookingManagmint.Models;

namespace BookingManagmint.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly DbConnectionFactory _factory;
        private readonly AppDbContext _db;
        public IndexModel(DbConnectionFactory factory, AppDbContext db)
        {
            _factory = factory;
            _db = db;
        }

        public List<TicketItem> Tickets { get; private set; } = new();
        public string? Message { get; private set; }
        public bool IsSuccess { get; private set; }

        public class TicketItem
        {
            public Guid Id { get; set; }
            public Guid EventId { get; set; }
            public string Status { get; set; } = string.Empty;
            public DateTime PurchasedAt { get; set; }
        }

        public void OnGet()
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdText, out var userId))
            {
                Tickets = new List<TicketItem>();
                return;
            }

            var rows = new List<TicketItem>();
            using var conn = _factory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TicketId, EventId, Status, PurchasedAt
                                FROM vw_UserTickets
                                WHERE UserId = @uid
                                ORDER BY PurchasedAt DESC";
            cmd.Parameters.Add(new SqlParameter("@uid", userId));
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new TicketItem
                {
                    Id = reader.GetGuid(0),
                    EventId = reader.GetGuid(1),
                    Status = StatusText(reader.GetInt32(2)),
                    PurchasedAt = reader.GetDateTime(3)
                });
            }
            Tickets = rows;
        }

        public IActionResult OnPostCancel(Guid ticketId)
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdText, out var userId))
            {
                Message = "Please log in to cancel tickets.";
                IsSuccess = false;
                OnGet();
                return Page();
            }

            try
            {
                using var conn = _factory.CreateConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                Guid eventId;
                using (var select = conn.CreateCommand())
                {
                    select.Transaction = tx;
                    select.CommandText = @"SELECT EventId FROM Tickets WHERE TicketId = @id AND UserId = @uid AND Status <> 2";
                    select.Parameters.Add(new SqlParameter("@id", ticketId));
                    select.Parameters.Add(new SqlParameter("@uid", userId));
                    var result = select.ExecuteScalar();
                    if (result == null)
                    {
                        tx.Rollback();
                        Message = "Ticket not found.";
                        IsSuccess = false;
                        OnGet();
                        return Page();
                    }
                    eventId = (Guid)result;
                }

                using (var updateTicket = conn.CreateCommand())
                {
                    updateTicket.Transaction = tx;
                    updateTicket.CommandText = @"UPDATE Tickets SET Status = 2 WHERE TicketId = @id";
                    updateTicket.Parameters.Add(new SqlParameter("@id", ticketId));
                    updateTicket.ExecuteNonQuery();
                }

                using (var release = conn.CreateCommand())
                {
                    release.Transaction = tx;
                    release.CommandText = @"UPDATE Events SET RemainingSeats = RemainingSeats + 1 WHERE EventId = @eventId";
                    release.Parameters.Add(new SqlParameter("@eventId", eventId));
                    release.ExecuteNonQuery();
                }

                tx.Commit();
                Message = "Ticket cancelled successfully.";
                IsSuccess = true;
                OnGet();
                return Page();
            }
            catch (Exception ex)
            {
                Message = $"Unexpected error: {ex.Message}";
                IsSuccess = false;
                OnGet();
                return Page();
            }
        }

        public IActionResult OnGetTickets()
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdText, out var userId))
            {
                Response.StatusCode = 401;
                return new JsonResult(Array.Empty<object>());
            }

            var rows = new List<object>();
            using var conn = _factory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT TicketId, EventId, Status, PurchasedAt
                                FROM vw_UserTickets
                                WHERE UserId = @uid
                                ORDER BY PurchasedAt DESC";
            cmd.Parameters.Add(new SqlParameter("@uid", userId));
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new
                {
                    id = reader.GetGuid(0),
                    eventId = reader.GetGuid(1),
                    status = StatusText(reader.GetInt32(2)),
                    purchasedAt = reader.GetDateTime(3)
                });
            }
            return new JsonResult(rows);
        }

        public IActionResult OnGetStats()
        {
            var totalTickets = _db.Tickets.Count();
            var paid = _db.Tickets.Count(t => t.Status == Models.TicketStatus.Paid);
            var upcoming = _db.Events.Count(e => e.DateTime >= DateTime.UtcNow);
            return new JsonResult(new { totalTickets, paidTickets = paid, upcomingEvents = upcoming });
        }

        private static string StatusText(int status) => status switch
        {
            0 => "Reserved",
            1 => "Paid",
            2 => "Cancelled",
            3 => "CheckedIn",
            _ => "Unknown"
        };
    }
}

