using BookingManagmint.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace BookingManagmint.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly DbConnectionFactory _factory;
        public LoginModel(DbConnectionFactory factory) => _factory = factory;

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var usernameOrEmail = Request.Form["UsernameOrEmail"].ToString().Trim();
            var password = Request.Form["Password"].ToString().Trim();

            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Please enter username/email and password.";
                return Page();
            }

            try
            {
                using var conn = _factory.CreateConnection();
                await conn.OpenAsync();
                
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT l.UserId, l.Username, l.PasswordHash, u.FullName, u.Email, u.Role
                    FROM Logins l
                    INNER JOIN Users u ON u.UserId = l.UserId
                    WHERE (l.Username = @input OR l.Email = @input) AND l.PasswordHash = @password
                ";
                cmd.Parameters.AddWithValue("@input", usernameOrEmail);
                cmd.Parameters.AddWithValue("@password", password);
                
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var userId = reader.GetGuid(0);
                    var fullName = reader.GetString(3);
                    var email = reader.GetString(4);
                    var role = reader.GetInt32(5);
                    
                    HttpContext.Session.SetString("UserId", userId.ToString());
                    HttpContext.Session.SetString("UserName", fullName);
                    HttpContext.Session.SetString("UserEmail", email);
                    HttpContext.Session.SetString("UserRole", role.ToString());
                    await HttpContext.Session.CommitAsync();
                    
                    return RedirectToPage("/Dashboard/Index");
                }
                else
                {
                    ErrorMessage = "Invalid username/email or password.";
                    return Page();
                }
            }
            catch
            {
                ErrorMessage = "An error occurred. Please try again.";
                return Page();
            }
        }
    }
}
