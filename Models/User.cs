using System;

namespace BookingManagmint.Models
{
    /// <summary>
    /// Represents a user in the booking system (Attendee, Organizer, or Admin).
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// User's full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// User's email address (must be unique).
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Hashed password (never store plain text).
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
        /// <summary>
        /// The user's assigned role (Attendee, Organizer, Admin).
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Default constructor for EF or deserialization.
        /// </summary>
        public User() { }
        /// <summary>
        /// Create a new user (usually from registration form).
        /// </summary>
        public User(string fullName, string email, string passwordHash, UserRole role)
        {
            UserId = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }
        
        /// <summary>
        /// Returns true if user is an organizer.
        /// </summary>
        public bool IsOrganizer() => Role == UserRole.Organizer;
        /// <summary>
        /// Returns true if user is an admin.
        /// </summary>
        public bool IsAdmin() => Role == UserRole.Admin;
    }
}
