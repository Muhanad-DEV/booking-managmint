using BookingManagmint.Data;
using BookingManagmint.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookingManagmint.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;
        public RegisterModel(AppDbContext context) => _context = context;

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
                // Check if email already exists
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == email);
                
                if (emailExists)
                {
                    ErrorMessage = "Email already exists. Please use a different email.";
                    return Page();
                }

                // Check if username already exists
                var usernameExists = await _context.Logins
                    .AnyAsync(l => l.Username == username);
                
                if (usernameExists)
                {
                    ErrorMessage = "Username already exists. Please choose a different username.";
                    return Page();
                }

                // Create new user
                var user = new User(fullName, email, password, UserRole.Attendee);
                _context.Users.Add(user);

                // Create login info
                var loginInfo = new LoginInfo
                {
                    LoginId = Guid.NewGuid(),
                    UserId = user.UserId,
                    Username = username,
                    PasswordHash = password,
                    Email = email,
                    Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Logins.Add(loginInfo);

                await _context.SaveChangesAsync();
                
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", ((int)UserRole.Attendee).ToString());
                await HttpContext.Session.CommitAsync();
                
                return RedirectToPage("/Dashboard/Index");
            }
            catch (DbUpdateException dbEx)
            {
                // Check for unique constraint violations
                if (dbEx.InnerException?.Message.Contains("UNIQUE") == true || 
                    dbEx.InnerException?.Message.Contains("duplicate") == true)
                {
                    ErrorMessage = "Email or username already exists. Please try a different one.";
                }
                else
                {
                    ErrorMessage = $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}";
                }
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                return Page();
            }
        }
    }
}
