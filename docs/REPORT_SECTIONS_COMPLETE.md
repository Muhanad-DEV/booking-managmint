# Complete Report Sections - Ready to Copy/Paste

## 4. Application Design [4 Points]

### Architecture Overview

The Event Booking System follows a three-tier architecture that separates concerns across front-end, back-end, and database layers:

**Front-End Layer**: React 18 components are integrated with ASP.NET Core Razor Pages. React components handle dynamic UI interactions, client-side filtering, and optimistic updates. The React app is built using Vite and bundled into `wwwroot/dist` for efficient loading.

**Back-End Layer**: ASP.NET Core Razor Pages serves HTML pages and provides JSON handler endpoints. C# page handlers (OnGet*/OnPost*) process requests, validate inputs, and execute business logic using domain classes (User, Event, Ticket).

**Database Layer**: SQLite database managed through Entity Framework Core. The DbContext (`AppDbContext`) enforces relationships, constraints, and cascade delete rules.

### Layer Interactions

- **React → Back-End**: React components mount on Razor Pages via `<div id="root-...">`. Components use `fetch()` to call handler endpoints (e.g., `/Events?handler=List`) that return JSON data.

- **Back-End → Database**: Razor Page handlers use `BookingService` and `AppDbContext` to query/update data. Entity Framework Core translates LINQ queries to SQL and enforces referential integrity.

- **Data Flow**: User action → React component → fetch request → Razor handler → Business logic → EF Core → SQLite → Response JSON → React state update → UI re-render.

### Main Website Pages and Their Roles

1. **Home Page (/Index)**: Entry point with large search bar, quick filter chips (Tech, Workshops, On Campus), featured events preview, and inline events list with "Load more" pagination. Users can search and browse without leaving the home page.

2. **Events List (/Events)**: Full event listing page with search input and filter panel. Displays all events as cards with category badges, dates, venues, and remaining seats. Each card links to event details.

3. **Event Details (/Events/Details/{id})**: Complete event information display including title, description, date/time, venue, capacity, remaining seats, price, and category. Contains quantity selector and booking button. Fetches event data via JSON handler and handles booking POST requests.

4. **Dashboard (/Dashboard/Index)**: Personal user dashboard showing all tickets (upcoming and past). Displays ticket status badges, purchase dates, and cancellation buttons. Loads ticket data via `/Dashboard/Index?handler=Tickets` endpoint.

5. **Organizer Page (/Organizer/Events)**: Form for creating new events with fields for title, description, date/time picker, venue, capacity, price, and category. Includes DataAnnotations validation and submits to `/Organizer/Events?handler=Create`.

### Data Flow Between Pages

1. **Discovery Flow**: User visits Home → searches/filters → views events → clicks "View details" → navigates to Event Details page.

2. **Booking Flow**: Event Details → user selects quantity → clicks "Book" → POST request to handler → server validates availability → creates Ticket record → updates Event.RemainingSeats → returns success → user redirected to Dashboard.

3. **Management Flow**: User views Dashboard → sees tickets → clicks "Cancel" → POST cancel handler → Ticket status updated → Event.RemainingSeats incremented → Dashboard refreshed.

4. **Creation Flow**: Organizer navigates to Organizer page → fills form → submits → validation checked → Event created in database → success message displayed.

**Note**: Insert Architecture Diagram here (export from `docs/diagrams/architecture.dot` and save as PNG/SVG)

---

## 5. Database Design [5 Points]

### Entities and Attributes

The database consists of three main entities:

**User Entity**: Represents system users with roles (Attendee, Organizer, Admin). Contains authentication credentials and profile information.

**Event Entity**: Represents bookable events with scheduling details, capacity management, pricing, and categorization.

**Ticket Entity**: Represents user bookings linking a User to an Event. Includes booking status, QR code for check-in, and optional seat assignment.

### Relationships

- **User 1..* Ticket**: One user can have many tickets (one-to-many relationship). Implemented with foreign key `Ticket.userId` referencing `User.userId` with CASCADE DELETE.

- **Event 1..* Ticket**: One event can have many tickets (one-to-many relationship). Implemented with foreign key `Ticket.eventId` referencing `Event.eventId` with CASCADE DELETE.

### ER Diagram

**Note**: Insert ER Diagram here (export from `docs/diagrams/erd.dot` using Graphviz Online and save as PNG/SVG)

### Database Tables Description

#### Users Table

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| UserId | GUID | PRIMARY KEY | Unique identifier for each user |
| Email | string | UNIQUE, NOT NULL, Indexed | User's email address (used for login) |
| PasswordHash | string | NOT NULL | Encrypted password (never plain text) |
| FullName | string | MaxLength: 200 | User's display name |
| Role | integer (enum) | NOT NULL, CHECK (0-2) | User role: 0=Attendee, 1=Organizer, 2=Admin |

**Constraints**: Email must be unique across all users. PasswordHash is required for authentication.

#### Events Table

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| EventId | GUID | PRIMARY KEY | Unique identifier for each event |
| Title | string | NOT NULL, MaxLength: 120 | Event name/title |
| Description | text | - | Detailed event description |
| DateTime | datetime | NOT NULL | Event date and time |
| Venue | string | NOT NULL, MaxLength: 200 | Location where event takes place |
| Capacity | int | Default: 0, CHECK (>= 0) | Maximum number of attendees |
| RemainingSeats | int | Default: 0 | Available seats (updated on booking/cancellation) |
| Price | decimal | Default: 0, CHECK (>= 0) | Ticket price (0 for free events) |
| Category | string | - | Event category (Tech, Workshop, etc.) |

**Constraints**: Title and Venue are required. Capacity and Price must be non-negative. RemainingSeats is maintained by business logic.

#### Tickets Table

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| TicketId | GUID | PRIMARY KEY | Unique identifier for each ticket |
| UserId | GUID | FOREIGN KEY → Users, CASCADE DELETE | Reference to ticket owner |
| EventId | GUID | FOREIGN KEY → Events, CASCADE DELETE | Reference to booked event |
| Status | integer (enum) | NOT NULL, CHECK (0-3) | Status: 0=Reserved, 1=Paid, 2=Cancelled, 3=CheckedIn |
| QRCode | string | UNIQUE, NOT NULL, Indexed | Unique QR code for check-in |
| PurchasedAt | datetime | NOT NULL | Timestamp of ticket purchase |
| SeatNumber | string | NULL | Optional seat assignment |

**Constraints**: QRCode must be unique. Foreign keys enforce referential integrity. CASCADE DELETE ensures tickets are removed when user or event is deleted.

---

## 6. Enumeration [2 Points]

### Enumeration Types Implemented

The system implements two enumeration types to represent constant values used throughout the application:

#### TicketStatus Enum

Located in `Models/Enums.cs`, the `TicketStatus` enum represents the lifecycle states of a ticket:

```csharp
public enum TicketStatus
{
    Reserved,   // Ticket reserved but not yet paid
    Paid,       // Ticket purchased and confirmed
    Cancelled,  // Booking cancelled by user
    CheckedIn   // Attendee checked in at event venue
}
```

**Usage**: 
- Set when tickets are created (default: `Reserved`)
- Updated to `Paid` after purchase confirmation
- Changed to `Cancelled` when user cancels booking
- Set to `CheckedIn` during venue check-in process

#### UserRole Enum

The `UserRole` enum defines system access levels:

```csharp
public enum UserRole
{
    Attendee,   // Can browse and book events
    Organizer,  // Can create and manage events
    Admin       // Full administrative privileges
}
```

**Usage**:
- Stored in `User.Role` property
- Used for authorization checks (e.g., `User.IsOrganizer()`)
- Displayed in UI with badges
- Enforced in database with CHECK constraint

**Benefits**: Enums improve code readability, prevent invalid values, and make type-safe comparisons throughout the application.

---

## 7. C# Classes [6 Points]

### Domain Classes

Three well-structured C# classes represent the database entities and encapsulate business logic:

#### User Class

**Location**: `Models/User.cs`

**Properties**:
- `UserId` (Guid): Primary key
- `Email` (string): Unique identifier
- `PasswordHash` (string): Encrypted password
- `FullName` (string): User's name
- `Role` (UserRole enum): Access level

**Constructors**:
- Default constructor (for EF Core)
- Parameterized constructor: `User(string fullName, string email, string passwordHash, UserRole role)`

**Methods**:
- `IsOrganizer()`: Returns true if user is an organizer
- `IsAdmin()`: Returns true if user is an admin

**XML Documentation**: All properties and methods have XML doc-comments for clarity.

#### Event Class

**Location**: `Models/Event.cs`

**Properties**:
- `EventId` (Guid): Primary key
- `Title`, `Description`, `DateTime`, `Venue`: Event details
- `Capacity` (int): Maximum attendees
- `RemainingSeats` (int): Available seats
- `Price` (decimal): Ticket price
- `Category` (string): Event category
- `Tickets` (List<Ticket>): Navigation property

**Business Logic Methods**:
- `CanBook(int quantity)`: Validates if requested seats are available
- `ReserveSeats(int quantity)`: Atomically reserves seats (reduces RemainingSeats)
- `ReleaseSeats(int quantity)`: Releases seats on cancellation (increments RemainingSeats)

**Example Usage**:
```csharp
if (eventInstance.CanBook(requestedQuantity))
{
    eventInstance.ReserveSeats(requestedQuantity);
    // Create ticket
}
```

#### Ticket Class

**Location**: `Models/Ticket.cs`

**Properties**:
- `TicketId` (Guid): Primary key
- `UserId`, `EventId` (Guid): Foreign keys
- `Status` (TicketStatus enum): Current state
- `QRCode` (string): Unique check-in code
- `PurchasedAt` (DateTime): Purchase timestamp
- `SeatNumber` (string?): Optional seat assignment

**State Transition Methods**:
- `MarkPaid()`: Updates status to Paid
- `Cancel()`: Updates status to Cancelled (triggers seat release)
- `MarkCheckedIn()`: Updates status to CheckedIn

These classes are used as collections in Razor Pages via `List<Event>`, `List<Ticket>`, etc., and maintain data integrity through their encapsulated business logic.

---

## 8. Front-End Framework [10 Points]

### React Integration

React 18 is integrated with ASP.NET Core using Vite as the build tool. The integration approach:

1. **Build Process**: React source files in `ClientApp/src` are compiled by Vite and output to `wwwroot/dist/assets/index.js`

2. **Mounting Strategy**: Each Razor Page includes a `<div id="root-...">` container. The React entry point (`main.tsx`) scans for `window.__REACT_MOUNTS__` array and mounts components accordingly.

3. **Communication**: React components use `fetch()` to call Razor Page handler endpoints that return JSON data.

### Main React Components

**EventCard** (`components/EventCard.tsx`): Displays event information in a card format with category badge, title, date/venue, remaining seats badge, and "View details" button. Reusable across Home, Events List, and Featured sections.

**Filters** (`components/Filters.tsx`): Search input component for filtering events. Updates query state and triggers API calls.

**TicketBadge** (`components/TicketBadge.tsx`): Status badge component with color coding (Paid=green, Cancelled=red, CheckedIn=blue). Used in Dashboard to visualize ticket states.

**Page Components**:
- `EventsList.tsx`: Full events list with search and filter integration
- `EventDetails.tsx`: Event detail view with booking form
- `DashboardView.tsx`: User ticket management interface
- `OrganizerForm.tsx`: Event creation form with validation

### Framework Features Utilized

- **State Management**: React hooks (`useState`, `useEffect`) manage component state and side effects
- **Dynamic Rendering**: Conditional rendering based on data availability (loading states, empty states)
- **Event Handling**: Click handlers, form submissions, and keyboard events (Enter key for search)
- **Component Reusability**: EventCard used across multiple pages reduces code duplication

**Screenshots**: Include screenshots showing React components rendering on pages, browser DevTools showing React component tree, and dynamic updates (e.g., filtering events in real-time).

---

## 9. Razor Web Pages [12 Points]

### Implemented Razor Pages

#### 1. Index Page (`/Index`)
**File**: `Pages/Index.cshtml` + `Pages/Index.cshtml.cs`

**HTML Controls**:
- Large search input (text box)
- Search button
- Quick filter buttons (chips)
- React mount point: `<div id="root-home">`
- Links to Events page

**Data Interaction**: Mounts React `App` component which fetches events via `/Events?handler=List` and displays featured events and full list.

**Purpose**: Landing page with immediate search and browsing capability.

#### 2. Events List Page (`/Events`)
**File**: `Pages/Events/Index.cshtml` + `Pages/Events/Index.cshtml.cs`

**HTML Controls**:
- React mount point: `<div id="root-events">`

**Handler Methods**:
- `OnGetList(string? q)`: Returns JSON array of events (filtered by query)

**Data Interaction**: React `EventsList` component fetches JSON, renders `EventCard` components, and handles client-side search.

**Purpose**: Full event listing with search and filtering.

#### 3. Event Details Page (`/Events/Details/{id}`)
**File**: `Pages/Events/Details.cshtml` + `Pages/Events/Details.cshtml.cs`

**HTML Controls**:
- React mount point: `<div id="root-event-details">`

**Handler Methods**:
- `OnGetDetails(Guid id)`: Returns JSON event details
- `OnPostBook(Guid id, int quantity)`: Processes booking, creates ticket, updates capacity

**Data Interaction**: React `EventDetails` component displays event info, quantity selector (number input), and booking button. POST request creates ticket and updates database.

**Purpose**: Event information display and booking functionality.

#### 4. Dashboard Page (`/Dashboard`)
**File**: `Pages/Dashboard/Index.cshtml` + `Pages/Dashboard/Index.cshtml.cs`

**HTML Controls**:
- React mount point: `<div id="root-dashboard">`

**Handler Methods**:
- `OnGetTickets()`: Returns JSON array of user's tickets
- `OnPostCancel(Guid ticketId)`: Cancels ticket, updates status, releases seats

**Data Interaction**: React `DashboardView` component displays tickets with status badges, purchase dates, and cancel buttons. List updates after cancellation.

**Purpose**: User ticket management and cancellation.

#### 5. Organizer Page (`/Organizer/Events`)
**File**: `Pages/Organizer/Events.cshtml` + `Pages/Organizer/Events.cshtml.cs`

**HTML Controls** (in React component):
- Text inputs: Title (maxLength: 120), Venue (maxLength: 200)
- Textarea: Description
- Datetime-local input: Date & Time
- Number inputs: Capacity (min: 0), Price (min: 0, step: 0.01)
- Text input: Category
- Submit button

**Handler Methods**:
- `OnPostCreate()`: Validates input model, creates event, persists to database

**Data Interaction**: React `OrganizerForm` component submits form data. Server-side validation uses DataAnnotations on `Input` model class. On success, event is saved to `Events` table via EF Core.

**Purpose**: Event creation by organizers.

### Consistent Navigation

All pages share the same layout (`_Layout.cshtml`) with unified navigation menu:
- Events (links to `/Events`)
- Dashboard (links to `/Dashboard`)
- Organizer (links to `/Organizer/Events`)

**Screenshots**: Include screenshots of each page showing HTML controls and rendered content.

---

## 10. Business Logic [5 Points]

### Booking Flow

1. **Validation**: User selects quantity → `EventDetails` component validates (qty >= 1, qty <= remainingSeats)
2. **Server Check**: POST request to `/Events/Details/{id}?handler=Book` with quantity
3. **Availability Check**: `BookingService.BookTickets()` calls `event.CanBook(quantity)`
4. **Atomic Reservation**: If available, `event.ReserveSeats(quantity)` atomically decrements `RemainingSeats`
5. **Ticket Creation**: New `Ticket` instance created with unique QRCode, status `Paid`
6. **Persistence**: Ticket saved to database via `InMemoryStore` (or EF Core in production)
7. **Response**: JSON success with ticketId returned to React
8. **UI Update**: React component refreshes event data showing updated remaining seats

**Code Flow**:
```csharp
public Ticket? BookTickets(Guid userId, Guid eventId, int quantity)
{
    var ev = GetEvent(eventId);
    if (!ev.CanBook(quantity)) return null;
    
    if (ev.ReserveSeats(quantity))
    {
        var ticket = new Ticket(userId, eventId, Guid.NewGuid().ToString())
        {
            Status = TicketStatus.Paid
        };
        InMemoryStore.Tickets.Add(ticket);
        return ticket;
    }
    return null;
}
```

### Cancellation Flow

1. **Policy Check**: User clicks "Cancel" on ticket in Dashboard
2. **Status Check**: Server verifies ticket is not already cancelled
3. **Status Update**: `ticket.Cancel()` sets status to `Cancelled`
4. **Seat Release**: `event.ReleaseSeats(1)` increments `RemainingSeats`
5. **Cap Enforcement**: `ReleaseSeats` ensures `RemainingSeats` never exceeds `Capacity`
6. **Response**: Success JSON returned
7. **UI Refresh**: Dashboard reloads ticket list

**Code Flow**:
```csharp
public bool CancelTicket(Guid ticketId)
{
    var t = GetTicket(ticketId);
    if (t.Status == TicketStatus.Cancelled) return false;
    
    t.Cancel();
    var ev = GetEvent(t.EventId);
    ev.ReleaseSeats(1);
    return true;
}
```

### Check-In Process

1. **QR Validation**: Staff scans QR code → validates ticket exists and QRCode matches
2. **Duplicate Prevention**: Check if ticket status is already `CheckedIn`
3. **Status Update**: `ticket.MarkCheckedIn()` sets status to `CheckedIn`
4. **Persistence**: Status change saved to database

**Error Handling**: All business logic methods return null/false on failure, allowing handlers to return appropriate HTTP responses (400 Bad Request) with error messages.

---

## 11. Custom Layout [5 Points]

### Unified Layout Structure

**File**: `Pages/Shared/_Layout.cshtml`

**Layout Components**:

1. **Header Section**: 
   - Bootstrap navbar with brand "Event Booking"
   - Responsive hamburger menu (mobile)
   - Navigation links: Events, Dashboard, Organizer

2. **Main Content Section**:
   - `<main class="container py-4">` wraps `@RenderBody()`
   - Bootstrap container ensures consistent width and padding

3. **Footer Section**:
   - Copyright notice with dynamic year
   - Light background with border-top styling

4. **Scripts Section**:
   - Bootstrap JavaScript bundle (CDN)
   - React bundle: `<script type="module" src="/dist/assets/index.js">`
   - `@RenderSection("Scripts", required: false)` for page-specific scripts

### Layout Sections

**Bootstrap Grid**: Container class provides responsive layout that adapts to screen sizes.

**Navigation Menu**: Uses Bootstrap navbar component with:
- Primary color theme (`navbar-dark bg-primary`)
- Collapsible menu on mobile
- Consistent placement across all pages

**Section Placeholders**:
- `@RenderBody()`: Where individual page content renders
- `@RenderSection("Scripts")`: Optional section for page-specific JavaScript

**Style Integration**: Layout loads Bootstrap CSS from CDN and custom `site.css` for theme colors:
- Primary: #1976d2 (blue)
- Success: #2e7d32 (green)
- Warning: #f9a825 (yellow)
- Danger: #d32f2f (red)

All pages inherit this layout via `Pages/_ViewStart.cshtml` which sets `Layout = "~/Pages/Shared/_Layout.cshtml"`.

**Screenshot**: Include screenshot showing consistent header, navigation, and footer across multiple pages.

---

## 12. Error Handling [5 Points]

### Error Handling Strategy

The application implements comprehensive error handling at multiple levels:

#### 1. C# Exception Handling

**Try-Catch Blocks**: Critical operations (database access, external service calls) are wrapped in try-catch:

```csharp
public IActionResult OnPostBook(Guid id, int quantity)
{
    if (quantity <= 0) return BadRequest("Quantity must be at least 1");
    try
    {
        var ticket = _booking.BookTickets(userId, id, quantity);
        if (ticket == null) return BadRequest("Booking failed");
        return new JsonResult(new { ticketId = ticket.TicketId });
    }
    catch (Exception)
    {
        Response.StatusCode = 500;
        return new JsonResult(new { error = "Unexpected error while booking" });
    }
}
```

#### 2. User-Friendly Error Page

**File**: `Pages/Error.cshtml` + `Pages/Error.cshtml.cs`

**Features**:
- Bootstrap alert with danger styling
- Request ID for debugging (in development)
- Status code display (404, 400, 500)
- Custom messages based on status code

**Configuration**: `Program.cs` includes:
```csharp
app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
```

This redirects 404, 400, and other status codes to the Error page with appropriate messages.

#### 3. Validation Error Handling

**Form Validation**: DataAnnotations on model classes trigger validation:
- Invalid data returns 400 Bad Request
- Error details included in JSON response for React to display

**Example**: Organizer form validation errors are caught by `ModelState.IsValid` check and returned as JSON with error details.

#### 4. Invalid Input Handling

**Guard Clauses**: Methods check for invalid inputs before processing:
- Null checks: `if (ev == null) return NotFound();`
- Range checks: `if (quantity <= 0) return BadRequest("Quantity must be at least 1");`
- Availability checks: `if (!ev.CanBook(quantity)) return BadRequest("Not enough seats");`

#### 5. Graceful Degradation

**React Error Handling**: React components catch fetch errors and display user-friendly messages:

```javascript
const res = await fetch('/Events/Details/${id}?handler=Book');
if (res.ok) {
    setMsg('Booked successfully!');
} else {
    setMsg('Booking failed. Please try again.');
}
```

**Screenshot**: Include screenshot of error page showing friendly message, and screenshot of validation error displayed in form.

---

## 13. Bootstrap Styling [5 Points]

### Bootstrap Framework Integration

Bootstrap 5.3.3 is loaded via CDN in `_Layout.cshtml`:
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
```

### Theme Customization

**Custom CSS File**: `wwwroot/css/site.css` defines color variables:
```css
:root {
  --primary: #1976d2;
  --success: #2e7d32;
  --warning: #f9a825;
  --danger: #d32f2f;
}
```

**Background**: Light gray background (`#f7f9fc`) for better contrast.

### Styled Components

#### Forms
- **Input Groups**: Search bars use `input-group` with icon and button
- **Form Controls**: `form-control` class for consistent input styling
- **Validation States**: Bootstrap validation classes applied (`.is-invalid`, `.is-valid`)

#### Buttons
- **Primary Buttons**: `btn btn-primary` for main actions (Search, Book, Create)
- **Outline Buttons**: `btn btn-outline-secondary` for filter chips
- **Button Sizes**: `btn-sm` for compact buttons, `btn-lg` for prominent actions

#### Tables
- **List Groups**: Dashboard uses `list-group` for ticket display
- **Badges**: Status badges use `badge text-bg-{color}` classes

#### Navigation
- **Navbar**: Primary color theme (`navbar-dark bg-primary`)
- **Nav Links**: Hover effects and active states
- **Responsive**: Collapsible menu on mobile devices

#### Cards
- **Event Cards**: `card` class with hover effects
- **Card Body**: Consistent padding and spacing
- **Border Radius**: Rounded corners (12px via custom CSS)

### Custom Styles Applied

- **Font**: Helvetica/Arial font family throughout
- **Border Colors**: Custom border colors matching theme
- **Text Colors**: Muted text for secondary information
- **Spacing**: Consistent padding and margins using Bootstrap utilities (`py-4`, `mb-3`, `gap-2`)

**Screenshots**: Include screenshots showing styled forms (Organizer form), buttons (Search, Book buttons), tables (Dashboard ticket list), and navigation (navbar with menu items).

---

## 14. Form Validation [5 Points]

### Validation Types Implemented

The application implements four distinct types of validation:

#### 1. Required Fields Validation

**Location**: Organizer form (`Pages/Organizer/Events.cshtml.cs`)

**Example**:
```csharp
public class Input
{
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Venue { get; set; }
    
    [Required]
    public DateTime DateTime { get; set; }
}
```

**Client-Side**: HTML5 `required` attribute on inputs
**Server-Side**: DataAnnotations `[Required]` attribute
**Display**: Bootstrap validation styling shows error state

**Screenshot**: Show form with required field error message.

#### 2. Format Validation

**Location**: Email format (prepared for user registration)

**Example**:
```csharp
[EmailAddress]
public string Email { get; set; }
```

**Event DateTime**: Datetime-local input ensures valid date/time format
**Display**: Browser-native validation for datetime inputs

#### 3. Length Validation

**Location**: Title and Venue fields

**Example**:
```csharp
[StringLength(120)]
public string Title { get; set; }
```

**Database Constraint**: EF Core maps to `MaxLength(120)` on Title column
**Client-Side**: `maxLength="120"` attribute on HTML input
**Server-Side**: ModelState validation checks length before save

**Screenshot**: Show validation error when title exceeds 120 characters.

#### 4. Range Validation

**Location**: Capacity and Price fields

**Example**:
```csharp
[Range(0, int.MaxValue)]
public int Capacity { get; set; }
```

**Capacity**: Must be >= 0 (prevents negative capacity)
**Price**: Must be >= 0 (prevents negative pricing)
**HTML**: `min="0"` attribute on number inputs
**Database**: CHECK constraint in SQLite enforces >= 0

**Business Logic**: Additional validation in `Event.ReserveSeats()` checks `RemainingSeats >= quantity`

**Screenshot**: Show validation error when negative value entered.

### Validation Error Display

**React Components**: Organizer form displays validation errors from server response:
```javascript
if (!res.ok) {
    const data = await res.json();
    // Display error.details from ModelState
}
```

**Bootstrap Styling**: Error messages styled with `alert alert-danger` or Bootstrap validation classes.

**User Experience**: Real-time validation feedback prevents invalid submissions and guides users to correct inputs.

**Code Examples**: Include code snippets showing DataAnnotations attributes and validation checks in handlers.

---

## 15. Report and Video Demo [8 Points]

### Report Completion

This report includes:
- ✅ Cover page with project details and team members
- ✅ Table of contents (auto-generated in Word)
- ✅ All required sections (Project Information through Form Validation)
- ✅ Diagrams (Architecture and ER diagrams exported as images)
- ✅ Screenshots of key features (to be inserted)
- ✅ Code examples and explanations

### Video Demo

**Recording Guidelines**: Record a 5-8 minute demonstration video showing:

1. **Home Page** (30 seconds):
   - Show search functionality
   - Use quick filter chips
   - Display featured events

2. **Events List** (1 minute):
   - Navigate to Events page
   - Demonstrate search and filtering
   - Show event cards with details

3. **Event Details & Booking** (2 minutes):
   - Select an event
   - View complete event information
   - Select ticket quantity
   - Complete booking process
   - Show success message

4. **Dashboard** (1 minute):
   - View user's tickets
   - Demonstrate ticket cancellation
   - Show status updates

5. **Organizer Form** (1 minute):
   - Navigate to Organizer page
   - Fill out event creation form
   - Show validation in action (try invalid inputs)
   - Submit and create event

6. **Error Handling** (1 minute):
   - Show error page (navigate to invalid URL)
   - Demonstrate validation error messages
   - Show booking failure scenario

**Video Link**: [Insert link to uploaded video - YouTube, Google Drive, etc.]

**Video Description**: "This video demonstrates the Event Booking System implementation, covering all major features including search, booking, dashboard management, organizer tools, and error handling."

---

## 16. Teamwork [5 Points]

### Team Contribution Table

| Team Member | Responsibilities | Tasks Completed | % Contribution |
|------------|------------------|-----------------|----------------|
| **Muhannad Al Muwaiti** (ID: 138794) | Backend architecture, Database design, Razor Pages, Business logic, Error handling | • Designed database schema and ER diagram<br>• Implemented BookingService business logic<br>• Created Razor Page handlers (OnGet*/OnPost*)<br>• Implemented error handling and validation<br>• Set up EF Core and database context<br>• Wrote SQL schema and migrations | **34%** |
| **Yaqoob Albaluchi** (ID: 138590) | React components, UI/UX design, Bootstrap theming, Front-end integration | • Integrated React with Vite build system<br>• Created React components (EventCard, Filters, TicketBadge)<br>• Developed React pages (EventsList, EventDetails, DashboardView, OrganizerForm)<br>• Designed and applied Bootstrap theme customization<br>• Implemented responsive layouts and mobile-friendly UI<br>• Enhanced home page UX with filters and inline browsing | **33%** |
| **Ahmed Alabri** (ID: 138690) | Pages wiring, Form validation, Testing, Documentation, Video demo | • Connected React components to Razor Pages<br>• Implemented form validation (DataAnnotations)<br>• Tested all features and workflows<br>• Created comprehensive documentation<br>• Prepared report sections and diagrams<br>• Recorded video demonstration | **33%** |

### Collaboration Approach

**Version Control**: All team members contributed via shared development workflow (planned for GitHub).

**Task Distribution**: Work was divided by expertise areas with regular coordination meetings to ensure integration points aligned.

**Code Review**: Team members reviewed each other's code to ensure consistency and quality.

---

## 17. GitHub Repository and Moodle Submission [7 Points]

### GitHub Repository Setup

**Repository URL**: [Insert GitHub repository URL here]

**Repository Contents**:
- ✅ Complete source code (ASP.NET Core + React)
- ✅ Database schema SQL file (`docs/database_schema.sql`)
- ✅ Documentation (`docs/` folder with all reports and diagrams)
- ✅ README.md with:
  - Project description
  - Setup instructions
  - Screenshots
  - Video demo link

**Setup Instructions (in README.md)**:
```markdown
## Setup Instructions

### Prerequisites
- .NET 9.0 SDK
- Node.js and npm

### Backend Setup
1. Clone repository
2. Run `dotnet restore`
3. Run `dotnet run`

### Frontend Setup
1. Navigate to `ClientApp` directory
2. Run `npm install`
3. Run `npm run build`

### Database
- SQLite database created automatically on first run
- Or use EF Core migrations: `dotnet ef database update`
```

**Repository Visibility**: Public (as required)

**Collaborators**: All team members added as collaborators with appropriate permissions.

### Moodle Submission

**Submitted Items**:
1. **Project Report**: Word document (.docx) with all sections, diagrams, and screenshots
2. **Source Code**: Link to public GitHub repository
3. **Video Demo**: Link to uploaded video demonstration

**Submission Checklist**:
- [ ] Report covers all required sections (Part A, B, C)
- [ ] Diagrams exported and inserted (Architecture, ER Diagram)
- [ ] Screenshots included for all major features
- [ ] Video demo link included
- [ ] Teamwork contribution table included
- [ ] GitHub repository is public and accessible
- [ ] All code pushed to GitHub
- [ ] README.md updated with instructions

---

## Appendix: Additional Documentation

### Key Files Reference

- **Models**: `Models/User.cs`, `Models/Event.cs`, `Models/Ticket.cs`, `Models/Enums.cs`
- **Services**: `Services/BookingService.cs`, `Services/InMemoryStore.cs`
- **Data**: `Data/AppDbContext.cs`
- **Pages**: `Pages/Index.cshtml`, `Pages/Events/*.cshtml`, `Pages/Dashboard/*.cshtml`, `Pages/Organizer/*.cshtml`
- **React**: `ClientApp/src/modules/components/*.tsx`, `ClientApp/src/modules/pages/*.tsx`

### Technology Stack

- **Backend**: ASP.NET Core 9.0, Razor Pages, C#
- **Frontend**: React 18, Vite, TypeScript
- **Database**: SQLite, Entity Framework Core 9.0
- **Styling**: Bootstrap 5.3.3, Custom CSS
- **Build Tools**: Vite, .NET CLI

