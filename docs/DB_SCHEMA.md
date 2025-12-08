# Database Schema and Migrations (SQLite + EF Core)

Connection: `Data Source=app.db`

Entities
- Users
  - userId (GUID, PK)
  - email (string, required, UNIQUE)
  - passwordHash (string, required)
  - role (enum: Attendee|Organizer|Admin)
  - fullName (string, max 200)
- Events
  - eventId (GUID, PK)
  - title (string, required, max 120)
  - description (text)
  - dateTime (datetime, required)
  - venue (string, required, max 200)
  - capacity (int, default 0, >=0)
  - remainingSeats (int, default 0)
  - price (decimal, default 0, >=0)
  - category (string)
- Tickets
  - ticketId (GUID, PK)
  - userId (GUID, FK -> Users ON DELETE CASCADE)
  - eventId (GUID, FK -> Events ON DELETE CASCADE)
  - status (enum: Reserved|Paid|Cancelled|CheckedIn)
  - qrCode (string, required, UNIQUE)
  - purchasedAt (datetime)
  - seatNumber (string, nullable)

Relationships
- User 1..* Ticket
- Event 1..* Ticket
- Optional Attendance (future): Ticket 0..1 Attendance

EF Core Model Configuration
- See `Data/AppDbContext.cs` for indexes and constraints.

Seeding (Prototype)
- In-memory seeding for demo: `Services/InMemoryStore.cs` (can be removed after migration to DB-only).

Migrations — Commands
Note: Ensure you have EF tools installed: `dotnet tool install --global dotnet-ef`

Run these from the project root (`booking-managmint.csproj` directory):

```bash
# add initial migration
DOTNET_ENVIRONMENT=Development dotnet ef migrations add InitialCreate --project . --startup-project .

# apply migrations to SQLite db
DOTNET_ENVIRONMENT=Development dotnet ef database update --project . --startup-project .

# to remove last migration (if needed)
dotnet ef migrations remove --project . --startup-project .
```

Switch from EnsureCreated
- `Program.cs` currently calls `EnsureCreated()` for simplicity. For production, remove it and rely solely on migrations.

Backup and Inspection
- The SQLite database file `app.db` will be created in the project directory. Use DB Browser for SQLite to inspect tables and constraints.
