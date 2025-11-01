# Event Booking System

Event Booking System for COMP4701 - Web Application Development

## Team Members

- Muhannad Al Muwaiti (ID: 138794)
- Yaqoob Albaluchi (ID: 138590)
- Ahmed Alabri (ID: 138690)

## Technologies

- **Backend**: ASP.NET Core 9.0 (Razor Pages)
- **Frontend**: React 18 + Vite
- **Database**: SQLite + Entity Framework Core
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

### Database

The SQLite database (`app.db`) is created automatically on first run via `EnsureCreated()`.

For production, use migrations:
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Project Structure

```
booking-managmint/
├── ClientApp/          # React frontend (Vite)
│   └── src/
│       └── modules/
│           ├── components/  # React components
│           └── pages/      # Page components
├── Data/               # Database context
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

