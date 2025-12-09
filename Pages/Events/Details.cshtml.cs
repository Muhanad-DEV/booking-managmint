using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookingManagmint.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace BookingManagmint.Pages.Events
{
    public class DetailsModel : PageModel
    {
        private readonly DbConnectionFactory _factory;
        public DetailsModel(DbConnectionFactory factory) => _factory = factory;

        public EventDetails? Event { get; private set; }
        public string? Message { get; private set; }
        public bool IsSuccess { get; private set; }

        public class EventDetails
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime DateTime { get; set; }
            public string Venue { get; set; } = string.Empty;
            public int Remaining { get; set; }
            public int Capacity { get; set; }
            public decimal Price { get; set; }
            public string Category { get; set; } = string.Empty;
        }

        public void OnGet(Guid id)
        {
            using var conn = _factory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT EventId, Title, Description, DateTime, Venue, RemainingSeats, Capacity, Price, Category
                                FROM Events
                                WHERE EventId = @id";
            cmd.Parameters.Add(new SqlParameter("@id", id));
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Event = new EventDetails
                {
                    Id = reader.GetGuid(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateTime = reader.GetDateTime(3),
                    Venue = reader.GetString(4),
                    Remaining = reader.GetInt32(5),
                    Capacity = reader.GetInt32(6),
                    Price = reader.GetDecimal(7),
                    Category = reader.GetString(8)
                };
            }
        }

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
                Message = "Please log in to book tickets.";
                IsSuccess = false;
                OnGet(id);
                return Page();
            }
            if (quantity <= 0)
            {
                Message = "Quantity must be at least 1.";
                IsSuccess = false;
                OnGet(id);
                return Page();
            }

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
                    Message = "Not enough seats available.";
                    IsSuccess = false;
                    OnGet(id);
                    return Page();
                }

                var ticketId = Guid.NewGuid();
                using var insert = conn.CreateCommand();
                insert.Transaction = tx;
                insert.CommandText = @"INSERT INTO Tickets (TicketId, UserId, EventId, Status, QRCode, SeatNumber)
                                       VALUES (@id, @userId, @eventId, @status, @qr, NULL)";
                insert.Parameters.Add(new SqlParameter("@id", ticketId));
                insert.Parameters.Add(new SqlParameter("@userId", userId));
                insert.Parameters.Add(new SqlParameter("@eventId", id));
                insert.Parameters.Add(new SqlParameter("@status", 1));
                insert.Parameters.Add(new SqlParameter("@qr", $"QR-{ticketId.ToString()[..8]}"));
                insert.ExecuteNonQuery();

                tx.Commit();
                Message = $"Booked! Ticket #{ticketId}";
                IsSuccess = true;
                OnGet(id);
                return Page();
            }
            catch (Exception ex)
            {
                Message = $"Unexpected error while booking: {ex.Message}";
                IsSuccess = false;
                OnGet(id);
                return Page();
            }
        }
    }
}

