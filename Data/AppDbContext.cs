using BookingManagmint.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingManagmint.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(x => x.UserId);
                b.Property(x => x.Email).IsRequired();
                b.HasIndex(x => x.Email).IsUnique();
                b.Property(x => x.FullName).HasMaxLength(200);
            });

            modelBuilder.Entity<Event>(b =>
            {
                b.HasKey(x => x.EventId);
                b.Property(x => x.Title).IsRequired().HasMaxLength(120);
                b.Property(x => x.Venue).IsRequired().HasMaxLength(200);
                b.Property(x => x.Capacity).HasDefaultValue(0);
                b.Property(x => x.RemainingSeats).HasDefaultValue(0);
                b.Property(x => x.Price).HasDefaultValue(0);
            });

            modelBuilder.Entity<Ticket>(b =>
            {
                b.HasKey(x => x.TicketId);
                b.Property(x => x.QRCode).IsRequired();
                b.HasIndex(x => x.QRCode).IsUnique();
                b.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

