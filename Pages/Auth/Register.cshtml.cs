using BookingManagmint.Data;
using BookingManagmint.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace BookingManagmint.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly DbConnectionFactory _factory;
        public RegisterModel(DbConnectionFactory factory) => _factory = factory;

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var fullName = Request.Form["FullName"].ToString().Trim();
            var email = Request.Form["Email"].ToString().Trim();
            var username = Request.Form["Username"].ToString().Trim();
            var password = Request.Form["Password"].ToString().Trim();
            var phone = Request.Form["Phone"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Please fill in all required fields.";
                return Page();
            }

            try
            {
                var userId = Guid.NewGuid();
                var loginId = Guid.NewGuid();
                
                using var conn = _factory.CreateConnection();
                await conn.OpenAsync();
                using var tx = conn.BeginTransaction();
                
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "INSERT INTO Users (UserId, FullName, Email, PasswordHash, Role) VALUES (@userId, @fullName, @email, @passwordHash, @role)";
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@fullName", fullName);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@passwordHash", password);
                        cmd.Parameters.AddWithValue("@role", (int)UserRole.Attendee);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = "INSERT INTO Logins (LoginId, UserId, Username, PasswordHash, Email, Phone, CreatedAt) VALUES (@loginId, @userId, @username, @passwordHash, @email, @phone, SYSUTCDATETIME())";
                        cmd.Parameters.AddWithValue("@loginId", loginId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@passwordHash", password);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrWhiteSpace(phone) ? DBNull.Value : (object)phone);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    
                    await tx.CommitAsync();
                    
                    HttpContext.Session.SetString("UserId", userId.ToString());
                    HttpContext.Session.SetString("UserName", fullName);
                    HttpContext.Session.SetString("UserEmail", email);
                    HttpContext.Session.SetString("UserRole", ((int)UserRole.Attendee).ToString());
                    await HttpContext.Session.CommitAsync();
                    
                    return RedirectToPage("/Dashboard/Index");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    ErrorMessage = "Email or username already exists. Please try a different one.";
                }
                else
                {
                    ErrorMessage = "Could not create account. Please try again.";
                }
                return Page();
            }
            catch
            {
                ErrorMessage = "An error occurred. Please try again.";
                return Page();
            }
        }
    }
}
