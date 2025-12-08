# Saved SQL Database Structure
**Saved for future reference - Application currently uses in-memory storage**

This document contains the complete SQL database structure that was previously used. The application has been converted to use in-memory storage only, but this structure is preserved for when you want to re-implement database support.

## Database Schema (SQLite)

### Connection String
```
Data Source=app.db
```

### Tables

#### Users Table
```sql
CREATE TABLE IF NOT EXISTS "Users" (
    "UserId" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "FullName" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("UserId"),
    CONSTRAINT "CK_Users_Role" CHECK ("Role" IN (0, 1, 2))
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");
```

**Fields:**
- `UserId` (TEXT, PK): Unique identifier (GUID as string)
- `Email` (TEXT, required, UNIQUE): User's email address
- `PasswordHash` (TEXT, required): Hashed password
- `FullName` (TEXT, max 200): User's full name
- `Role` (INTEGER): User role enum (0=Attendee, 1=Organizer, 2=Admin)

#### Events Table
```sql
CREATE TABLE IF NOT EXISTS "Events" (
    "EventId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "DateTime" TEXT NOT NULL,
    "Venue" TEXT NOT NULL,
    "Capacity" INTEGER NOT NULL DEFAULT 0,
    "RemainingSeats" INTEGER NOT NULL DEFAULT 0,
    "Price" TEXT NOT NULL DEFAULT '0.0',
    "Category" TEXT NOT NULL,
    CONSTRAINT "PK_Events" PRIMARY KEY ("EventId"),
    CONSTRAINT "CK_Events_Capacity" CHECK ("Capacity" >= 0),
    CONSTRAINT "CK_Events_RemainingSeats" CHECK ("RemainingSeats" >= 0),
    CONSTRAINT "CK_Events_Price" CHECK (CAST("Price" AS REAL) >= 0)
);
```

**Fields:**
- `EventId` (TEXT, PK): Unique identifier (GUID as string)
- `Title` (TEXT, required, max 120): Event title
- `Description` (TEXT): Event description
- `DateTime` (TEXT): Event date and time (ISO format)
- `Venue` (TEXT, required, max 200): Event venue
- `Capacity` (INTEGER, default 0): Total capacity
- `RemainingSeats` (INTEGER, default 0): Available seats
- `Price` (TEXT, default '0.0'): Ticket price (stored as text, cast to REAL)
- `Category` (TEXT): Event category

#### Tickets Table
```sql
CREATE TABLE IF NOT EXISTS "Tickets" (
    "TicketId" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "EventId" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "QRCode" TEXT NOT NULL,
    "PurchasedAt" TEXT NOT NULL,
    "SeatNumber" TEXT NULL,
    CONSTRAINT "PK_Tickets" PRIMARY KEY ("TicketId"),
    CONSTRAINT "FK_Tickets_Events_EventId" FOREIGN KEY ("EventId") 
        REFERENCES "Events" ("EventId") ON DELETE CASCADE,
    CONSTRAINT "FK_Tickets_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("UserId") ON DELETE CASCADE,
    CONSTRAINT "CK_Tickets_Status" CHECK ("Status" IN (0, 1, 2, 3))
);

CREATE INDEX IF NOT EXISTS "IX_Tickets_EventId" ON "Tickets" ("EventId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Tickets_QRCode" ON "Tickets" ("QRCode");
CREATE INDEX IF NOT EXISTS "IX_Tickets_UserId" ON "Tickets" ("UserId");
```

**Fields:**
- `TicketId` (TEXT, PK): Unique identifier (GUID as string)
- `UserId` (TEXT, FK): Reference to Users table (CASCADE DELETE)
- `EventId` (TEXT, FK): Reference to Events table (CASCADE DELETE)
- `Status` (INTEGER): Ticket status enum (0=Reserved, 1=Paid, 2=Cancelled, 3=CheckedIn)
- `QRCode` (TEXT, required, UNIQUE): QR code for ticket
- `PurchasedAt` (TEXT): Purchase timestamp (ISO format)
- `SeatNumber` (TEXT, nullable): Seat number if assigned

### Entity Framework Core Configuration

The previous `AppDbContext.cs` configuration:

```csharp
public class AppDbContext : DbContext
{
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
```

### Relationships

- **User 1..* Ticket**: One user can have many tickets
- **Event 1..* Ticket**: One event can have many tickets
- Foreign keys use CASCADE DELETE (deleting a user/event deletes their tickets)

### Enum Values

**UserRole:**
- 0 = Attendee
- 1 = Organizer
- 2 = Admin

**TicketStatus:**
- 0 = Reserved
- 1 = Paid
- 2 = Cancelled
- 3 = CheckedIn

### Program.cs Configuration (Previous)

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
```

### Required NuGet Packages (Previous)

- `Microsoft.EntityFrameworkCore` (Version 9.0.0)
- `Microsoft.EntityFrameworkCore.Sqlite` (Version 9.0.0)
- `Microsoft.EntityFrameworkCore.Design` (Version 9.0.0)

### Migration Commands (For Future Reference)

```bash
# Install EF tools
dotnet tool install --global dotnet-ef

# Add initial migration
DOTNET_ENVIRONMENT=Development dotnet ef migrations add InitialCreate --project . --startup-project .

# Apply migrations
DOTNET_ENVIRONMENT=Development dotnet ef database update --project . --startup-project .

# Remove last migration (if needed)
dotnet ef migrations remove --project . --startup-project .
```

---

**Note:** This structure was saved on conversion to in-memory storage. To re-implement database support, restore the `Data/AppDbContext.cs` file, add back the EF Core packages, and update `Program.cs` and any page models that use the database context.

