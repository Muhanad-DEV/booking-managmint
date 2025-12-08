# Event Booking System

Event Booking System for COMP4701 - Web Application Development

## Team Members

- Muhannad Al Muwaiti (ID: 138794)
- Yaqoob Albaluchi (ID: 138590)
- Ahmed Alabri (ID: 138690)

## Technologies

- **Backend**: ASP.NET Core 9.0 (Razor Pages)
- **Frontend**: React 18 + Vite
- **Storage**: SQL Server (ADO.NET for direct SQL, EF Core for LINQ/ORM)
- **Styling**: Bootstrap 5.3.3

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK
- Node.js (v18+) and npm

### Backend Setup

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

   The app will be available at:
   - http://localhost:5000
   - https://localhost:5001

3. Prepare SQL Server database:
   ```bash
   # ensure DefaultConnection in appsettings.json points to your SQL Server
   sqlcmd -S "(localdb)\\MSSQLLocalDB" -i docs/sqlserver_init.sql
   ```

   The script creates tables, constraints, a view, and seeds sample data.

### Frontend Setup

1. Navigate to ClientApp directory:
   ```bash
   cd ClientApp
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Build React app:
   ```bash
   npm run build
   ```

   This outputs to `wwwroot/dist` for the Razor Pages to load.

### Storage

The application now uses **SQL Server**.

- Connection string: set `DefaultConnection` in `appsettings.json` (defaults to `(localdb)\\MSSQLLocalDB`).
- Initialize schema and seed data: run the basic script `docs/sqlserver_init.sql` against your SQL Server instance.
- Direct SQL pages (Question 2): Events list, Event details/booking, Dashboard tickets use ADO.NET with parameterized commands.
- EF/LINQ pages (Question 3): Auth (login/register/logout), Organizer events, Dashboard stats, Home summary use EF Core and LINQ.

## Project Structure

```
booking-managmint/
├── ClientApp/          # React frontend (Vite)
│   └── src/
│       └── modules/
│           ├── components/  # React components
│           └── pages/      # Page components
├── Services/           # Business logic (includes InMemoryStore)
├── Models/             # Domain models (User, Event, Ticket)
├── Pages/              # Razor Pages
│   ├── Events/        # Events listing and details
│   ├── Dashboard/     # User dashboard
│   └── Organizer/     # Event creation
├── Services/           # Business logic
├── wwwroot/            # Static files (includes React build)
└── Program.cs          # Application entry point
```

## Features

- Browse and search events
- Book tickets with quantity selection
- User dashboard for ticket management
- Event cancellation
- Organizer event creation
- Responsive Bootstrap UI
- React-based dynamic interactions

## Demo Video

[Insert link to demo video here]

## License

Academic project for COMP4701 course.

