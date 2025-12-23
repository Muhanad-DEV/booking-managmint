using System.ComponentModel.DataAnnotations;
using BookingManagmint.Models;

namespace BookingManagmint.ViewModels
{
    public class EventViewModel
    {
        public Guid EventId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(120, ErrorMessage = "Title cannot exceed 120 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(400, ErrorMessage = "Description cannot exceed 400 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date and time is required")]
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [StringLength(200, ErrorMessage = "Venue cannot exceed 200 characters")]
        public string Venue { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be 0 or greater")]
        public int Capacity { get; set; }

        public int RemainingSeats { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(80, ErrorMessage = "Category cannot exceed 80 characters")]
        public string Category { get; set; } = "General";
    }

    public class EventSearchViewModel
    {
        public string? SearchQuery { get; set; }
        public string? Category { get; set; }
        public List<EventViewModel> Events { get; set; } = new();
    }
}


