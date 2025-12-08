using System.ComponentModel.DataAnnotations;
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
        private readonly AppDbContext _db;
        public RegisterModel(AppDbContext db) => _db = db;

        public class InputModel
        {
            [Required, StringLength(200)]
            public string FullName { get; set; } = string.Empty;

            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, StringLength(100)]
            public string Username { get; set; } = string.Empty;

            [Required, StringLength(100, MinimumLength = 4)]
            public string Password { get; set; } = string.Empty;

            [Phone]
            public string? Phone { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var emailExists = await _db.Users.AnyAsync(u => u.Email == Input.Email);
            var userNameExists = await _db.Logins.AnyAsync(l => l.Username == Input.Username);
            if (emailExists || userNameExists)
            {
                ModelState.AddModelError(string.Empty, "User already exists");
                return Page();
            }

            var user = new User(Input.FullName, Input.Email, Input.Password, UserRole.Attendee);
            var login = new LoginInfo
            {
                LoginId = Guid.NewGuid(),
                UserId = user.UserId,
                Username = Input.Username,
                PasswordHash = Input.Password,
                Email = Input.Email,
                Phone = Input.Phone,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.Logins.Add(login);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", ((int)user.Role).ToString());

            return RedirectToPage("/Index");
        }
    }
}

