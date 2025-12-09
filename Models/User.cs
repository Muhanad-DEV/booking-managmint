using System;

namespace BookingManagmint.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public User() { }
        public User(string fullName, string email, string passwordHash, UserRole role)
        {
            UserId = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }
        
        public bool IsOrganizer() => Role == UserRole.Organizer;
        public bool IsAdmin() => Role == UserRole.Admin;
    }
}
