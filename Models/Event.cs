using System;
using System.Collections.Generic;

namespace BookingManagmint.Models
{
    public class Event
    {
        public Guid EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int RemainingSeats { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();

        public Event() { }

        public Event(string title, string desc, DateTime dt, string venue, int cap, decimal price, string cat)
        {
            EventId = Guid.NewGuid();
            Title = title;
            Description = desc;
            DateTime = dt;
            Venue = venue;
            Capacity = cap;
            RemainingSeats = cap;
            Price = price;
            Category = cat;
        }

        public bool CanBook(int quantity) => RemainingSeats >= quantity;

        public bool ReserveSeats(int quantity)
        {
            if (CanBook(quantity))
            {
                RemainingSeats -= quantity;
                return true;
            }
            return false;
        }
        public void ReleaseSeats(int quantity)
        {
            RemainingSeats += quantity;
            if (RemainingSeats > Capacity) RemainingSeats = Capacity;
        }
    }
}
