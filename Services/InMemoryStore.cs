using System;
using System.Collections.Generic;
using BookingManagmint.Models;

namespace BookingManagmint.Services
{
    public static class InMemoryStore
    {
        private static readonly object Sync = new object();

        public static List<User> Users { get; } = new List<User>();
        public static List<Event> Events { get; } = new List<Event>();
        public static List<Ticket> Tickets { get; } = new List<Ticket>();

        static InMemoryStore()
        {
            var organizer = new User("Organizer One", "org@example.com", "<hashed>", UserRole.Organizer);
            var attendee = new User("Attendee One", "user@example.com", "<hashed>", UserRole.Attendee);
            Users.Add(organizer);
            Users.Add(attendee);

            var e1 = new Event("Tech Talk", "Intro to ASP.NET Core", DateTime.UtcNow.AddDays(7), "Auditorium A", 50, 0, "Tech") { Price = 0 };
            var e2 = new Event("Workshop", "React Basics", DateTime.UtcNow.AddDays(14), "Lab 1", 30, 0, "Workshop") { Price = 0 };
            Events.Add(e1);
            Events.Add(e2);
        }

        public static void WithLock(Action action)
        {
            lock (Sync)
            {
                action();
            }
        }
    }
}

