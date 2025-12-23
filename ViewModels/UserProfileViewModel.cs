using System.ComponentModel.DataAnnotations;

namespace BookingManagmint.ViewModels
{
    public class UserProfileViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}


