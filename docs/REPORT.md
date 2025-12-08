# Event Booking System — Project Report

Cover Page
- Project: Event Booking System
- Course: COMP4701 – Web Application Development (Fall 2025)
- University: Sultan Qaboos University
- Instructor: Dr. Abdullah
- Team Members:
  - Muhannad Al Muwaiti (ID: 138794)
  - Yaqoob Albaluchi (ID: 138590)
  - Ahmed Alabri (ID: 138690)
- Date: 27 October 2025

Table of Contents
- Use Word’s automatic TOC after pasting (apply Heading styles to each section)

## 1. Project Information
### Title
Event Booking System

### Objectives
- Design and develop an online platform for browsing, booking, and managing event reservations.
- Enable organizers to create and manage events (date, time, venue, capacity, ticket types).
- Allow users to register, purchase securely, and receive automated confirmations/reminders.
- Minimize manual work and booking errors; enhance UX with a responsive, dynamic UI.
- Support seat selection, cancellation, and attendance tracking.

### Motivation and Need
Managing events is common across universities and organizations, but processes are often manual (forms, spreadsheets). This causes double bookings, delays, and poor communication. The proposed system centralizes event discovery and booking, automates confirmations, and improves reliability and efficiency—especially relevant post‑COVID with increased demand for digital services.

### Real‑World Benefits
- Organizers: Simplified management, reduced workload, real‑time stats.
- Attendees: Easy booking, instant confirmations, secure payments.
- Institutions: Increased participation, reduced paper use.

### Potential Users
- Event organizers, students, staff, administrative teams, public/community members.

## 2. User Interviews
- Attendee (Ayoob Alabri): wants a single platform, clear event details, instant confirmation/reminders, easy cancellation.
- Administrator (Mr. Abdullah Albulochi): needs seat limits, dashboard, reports, QR check‑in, fewer duplicates/overbooking.
Impact on requirements: centralized listings, confirmations/reminders, seat locking, dashboards, attendance scanning.

## 3. Project Requirements
### Functional
1. Accounts & Authentication (register, login, logout, reset)
2. Role Management (Attendee, Organizer, Admin)
3. Browse Events (search, filters by date/category/location)
4. Event Details (title, description, date/time, venue, capacity, remaining seats, tickets, price)
5. Ticket/Seat Selection (temporary locking to avoid double booking)
6. Ticket Issuance (unique code; update capacity)
7. Notifications/Reminders (emails on booking/changes)
8. Cancellations (policy‑based; update capacity/status)
9. User Dashboard (upcoming, tickets, orders, cancellations)
10. Organizer Dashboard (create/edit/publish events, manage capacity/images, performance)
11. Attendance/Check‑In (scan code; prevent duplicates)
12. Data Validation (required fields, formats, ranges)

### Non‑Functional
- Usability (first‑time booking in ~3 minutes)
- Performance (list < 3s; actions < 1s)
- Reliability (≥ 99% up‑time target)
- Data Accuracy (consistent and up‑to‑date)
- Security & Privacy (hashed passwords, HTTPS)
- Scalability (handle concurrent users)
- Maintainability (clear structure, docs)

## 4. Application Design
### Architecture
- ASP.NET Core Razor Pages with React components for dynamic UI.
- EF Core + SQLite for persistence.
- Email/notifications (placeholder for integration).

Insert diagram (export from Graphviz): docs/diagrams/architecture.dot

### Interactions
- React renders on Razor pages and fetches JSON handlers (OnGet*/OnPost*).
- Backend enforces validation and business rules; updates DB.
- Data flows: Events list → Event details → Booking → Email/confirmation → Dashboard → Check‑in.

### Main Pages
- Events List (/Events): search/filter, list of cards.
- Event Details (/Events/Details/{id}): info, quantity, booking.
- Dashboard (/Dashboard): list/cancel tickets.
- Organizer (/Organizer/Events): create/edit events.

## 5. Database Design
Entities: User, Event, Ticket. Relations: User 1..* Ticket; Event 1..* Ticket.

Insert ER diagram (export from Graphviz): docs/diagrams/erd.dot

Tables + constraints (highlights):
- Users: userId PK, email UNIQUE, fullName (<=200)
- Events: eventId PK, title REQ (<=120), venue REQ (<=200), capacity >=0, remainingSeats >=0, price >=0
- Tickets: ticketId PK, userId FK, eventId FK, qrCode UNIQUE, status ENUM

## 6. Implementation (Part B)
- Enums: TicketStatus, UserRole.
- C# Classes: User, Event, Ticket with constructors and business methods.
- Razor Pages: `/Events`, `/Events/Details`, `/Dashboard`, `/Organizer/Events`.
- React Components: EventCard, Filters, TicketBadge; pages for EventsList, EventDetails, DashboardView, OrganizerForm.
- Business Logic: booking (reserve, create ticket), cancellation (release seats), check‑in; guarded with try/catch and validation.
- Layout: `_Layout.cshtml` + Bootstrap theme.
- Error Handling: centralized Error page, status code re‑execute, friendly messages.
- Validation: DataAnnotations (Required, StringLength, Range), JSON 400 with details for React.

## 7. Testing & Demo
- Scenarios: create event, list events, book ticket, view dashboard, cancel a ticket, try invalid inputs.
- Screenshots: [Paste screenshots of each page here]
- Demo video link: [Insert link]

## 8. Teamwork
| Member | Responsibilities | % |
|---|---|---|
| Muhannad Al Muwaiti | Backend logic, DB design, Razor handlers | 34 |
| Yaqoob Albaluchi | React components, UI/UX, Bootstrap theming | 33 |
| Ahmed Alabri | Pages wiring, validation, testing & demo | 33 |

## 9. Repository & Submission
- Public GitHub: [insert URL]
- Build/Run:
  - Backend: `dotnet run`
  - Frontend: `cd ClientApp && npm i && npm run build`
- Migrations:
  - `dotnet tool install --global dotnet-ef`
  - `dotnet ef migrations add InitialCreate`
  - `dotnet ef database update`

Appendix
- Key files: `Program.cs`, `Data/AppDbContext.cs`, `Pages/*`, `ClientApp/*`.
- Troubleshooting: ensure `wwwroot/dist` exists (run Vite build); confirm `app.db` created; check console/network for handler errors.
