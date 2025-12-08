using System.ComponentModel.DataAnnotations;
using BookingManagmint.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BookingManagmint.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _db;
        public LoginModel(AppDbContext db) => _db = db;

        public class InputModel
        {
            [Required]
            public string UsernameOrEmail { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var login = await _db.Logins
                .FirstOrDefaultAsync(l =>
                    (l.Username == Input.UsernameOrEmail || l.Email == Input.UsernameOrEmail) &&
                    l.PasswordHash == Input.Password);

            if (login == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == login.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found");
                return Page();
            }

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", ((int)user.Role).ToString());

            return RedirectToPage("/Index");
        }
    }
}

