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

- .NET 9.0 SDK (for local development)
- Node.js (v18+) and npm (for frontend)
- Docker and Docker Compose (for containerized deployment)

### Docker Setup (Recommended)

**Prerequisites:** Install Docker Desktop from https://www.docker.com/products/docker-desktop/

**If `docker` command is not found:**
- **Option 1 (Easiest):** Use the helper script:
  ```bash
  ./run-docker.sh up --build
  ```
  
- **Option 2:** Add Docker to PATH for this session:
  ```bash
  export PATH="/Applications/Docker.app/Contents/Resources/bin:$PATH"
  docker compose up --build
  ```
  
- **Option 3:** Restart your terminal (Docker Desktop should add it to PATH automatically)

1. Build and run with Docker Compose:
   ```bash
   docker compose up --build
   ```
   
   (Use `docker-compose` if you have the standalone version)

2. The application will be available at:
   - http://localhost:8080

3. The database is automatically initialized on first startup. SQL Server will be available at:
   - Server: `localhost,1433`
   - Username: `sa`
   - Password: `YourStrong!Passw0rd123`

4. To stop the containers:
   ```bash
   docker compose down
   ```

5. To remove volumes (clean database):
   ```bash
   docker compose down -v
   ```

### Local Development Setup

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

