using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace BookingManagmint.Pages.Events
{
    [IgnoreAntiforgeryToken]
    public class DetailsModel : PageModel
    {
        private readonly DbConnectionFactory _factory;
        public DetailsModel(DbConnectionFactory factory) => _factory = factory;

        public IActionResult OnGetDetails(Guid id)
        {
            using var conn = _factory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT EventId, Title, Description, DateTime, Venue, RemainingSeats, Capacity, Price, Category
                                FROM Events
                                WHERE EventId = @id";
            cmd.Parameters.Add(new SqlParameter("@id", id));
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return NotFound();

            var result = new
            {
                id = reader.GetGuid(0),
                title = reader.GetString(1),
                description = reader.GetString(2),
                dateTime = reader.GetDateTime(3),
                venue = reader.GetString(4),
                remaining = reader.GetInt32(5),
                capacity = reader.GetInt32(6),
                price = reader.GetDecimal(7),
                category = reader.GetString(8)
            };
            return new JsonResult(result);
        }

        public IActionResult OnPostBook(Guid id, int quantity = 1)
        {
            var userIdText = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdText, out var userId))
            {
                Response.StatusCode = 401;
                return new JsonResult(new { error = "Not logged in" });
            }
            if (quantity <= 0) return BadRequest("Quantity must be at least 1");

            try
            {
                using var conn = _factory.CreateConnection();
                conn.Open();
                using var tx = conn.BeginTransaction();

                using var update = conn.CreateCommand();
                update.Transaction = tx;
                update.CommandText = @"UPDATE Events
                                       SET RemainingSeats = RemainingSeats - @qty
                                       WHERE EventId = @eventId AND RemainingSeats >= @qty";
                update.Parameters.Add(new SqlParameter("@qty", quantity));
                update.Parameters.Add(new SqlParameter("@eventId", id));

                var rows = update.ExecuteNonQuery();
                if (rows == 0)
                {
                    tx.Rollback();
                    return BadRequest("Not enough seats available");
                }

                var ticketId = Guid.NewGuid();
                using var insert = conn.CreateCommand();
                insert.Transaction = tx;
                insert.CommandText = @"INSERT INTO Tickets (TicketId, UserId, EventId, Status, QRCode, SeatNumber)
                                       VALUES (@id, @userId, @eventId, @status, @qr, NULL)";
                insert.Parameters.Add(new SqlParameter("@id", ticketId));
                insert.Parameters.Add(new SqlParameter("@userId", userId));
                insert.Parameters.Add(new SqlParameter("@eventId", id));
                insert.Parameters.Add(new SqlParameter("@status", 1)); // Paid
                insert.Parameters.Add(new SqlParameter("@qr", $"QR-{ticketId.ToString()[..8]}"));
                insert.ExecuteNonQuery();

                tx.Commit();
                return new JsonResult(new { ticketId, status = "Paid" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return new JsonResult(new { error = "Unexpected error while booking", detail = ex.Message });
            }
        }
    }
}

