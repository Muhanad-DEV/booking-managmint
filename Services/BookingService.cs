using System;
using System.Collections.Generic;
using System.Linq;
using BookingManagmint.Models;

namespace BookingManagmint.Services
{
    public class BookingService
    {
        public IEnumerable<Event> ListEvents(string? query = null)
        {
            var q = InMemoryStore.Events.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(e => e.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
            }
            return q.OrderBy(e => e.DateTime);
        }

        public Event? GetEvent(Guid id) => InMemoryStore.Events.FirstOrDefault(e => e.EventId == id);

        public Ticket? BookTickets(Guid userId, Guid eventId, int quantity)
        {
            if (quantity <= 0) return null;
            var ev = GetEvent(eventId);
            if (ev == null) return null;

            Ticket? ticket = null;
            InMemoryStore.WithLock(() =>
            {
                if (!ev.CanBook(quantity)) return;
                if (!ev.ReserveSeats(quantity)) return;
                ticket = new Ticket(userId, eventId, Guid.NewGuid().ToString())
                {
                    Status = TicketStatus.Paid
                };
                InMemoryStore.Tickets.Add(ticket);
            });
            return ticket;
        }

        public bool CancelTicket(Guid ticketId)
        {
            var t = InMemoryStore.Tickets.FirstOrDefault(x => x.TicketId == ticketId);
            if (t == null) return false;
            var ev = GetEvent(t.EventId);
            if (ev == null) return false;

            InMemoryStore.WithLock(() =>
            {
                if (t.Status == TicketStatus.Cancelled) return;
                t.Cancel();
                ev.ReleaseSeats(1);
            });
            return true;
        }

        public bool CheckIn(Guid ticketId)
        {
            var t = InMemoryStore.Tickets.FirstOrDefault(x => x.TicketId == ticketId);
            if (t == null) return false;
            if (t.Status == TicketStatus.CheckedIn) return false;
            t.MarkCheckedIn();
            return true;
        }

        public IEnumerable<Ticket> ListUserTickets(Guid userId)
        {
            return InMemoryStore.Tickets.Where(t => t.UserId == userId);
        }

        public Event CreateEvent(string title, string description, DateTime dateTime, string venue, int capacity, decimal price, string category)
        {
            var ev = new Event(title, description, dateTime, venue, capacity, price, category);
            InMemoryStore.WithLock(() => InMemoryStore.Events.Add(ev));
            return ev;
        }
    }
}

