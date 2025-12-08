# Proposed User Workflow (v2) — Review Before Implementing

Goal: Make discovery → booking → managing tickets frictionless, and simplify organizer tasks.

## 1) Home (/)
- Large search bar (title, venue, category); Enter to search.
- Quick chips: Tech, Workshop, On Campus.
- Featured (top 3 upcoming) summary cards.
- CTA buttons: "Browse all events" and "Create event" (if organizer).

System actions:
- GET /Events?handler=List to hydrate featured.

## 2) Events List (/Events)
- Left: Filters panel (search, category, date range, venue), sticky on desktop.
- Right: Paginated cards (title, date/time, venue, remaining, category badge).
- Sorting: Soonest, Popular, Price.

System actions:
- GET /Events?handler=List&q=&category=&from=&to=&venue=

## 3) Event Details (/Events/Details/{id})
- Full info + remaining seats + price.
- Qty selector with validation; Book button.
- Success toast with ticket number and link to Dashboard.

System actions:
- GET /Events/Details/{id}?handler=Details
- POST /Events/Details/{id}?handler=Book (qty)
- Rules: quantity ≥1, remaining ≥ qty; update remainingSeats; create Ticket (Paid) with qrCode.

## 4) Dashboard (/Dashboard)
- Tabs: Upcoming, Past, Cancelled.
- Row actions: Cancel (policy checks), Download QR (future), View details.

System actions:
- GET /Dashboard/Index?handler=Tickets
- POST /Dashboard/Index?handler=Cancel (ticketId)

## 5) Organizer (/Organizer/Events)
- Create form: title, date/time, venue, capacity, price, category, description.
- Manage table (future): edit, unpublish, stats.

System actions:
- POST /Organizer/Events?handler=Create (DataAnnotations validation)

## 6) UX/Validation
- Client: required, type/format, min/max; inline errors.
- Server: DataAnnotations + guard clauses; JSON 400 with {error, details}.
- Error page: friendly messages; status reroute.

## 7) Data/DB
- EF Core + SQLite: Users, Events, Tickets (as documented in DB_SCHEMA.md).
- Migrations (preferred) over EnsureCreated for final submission.

## 8) Styling/Layout
- Bootstrap 5 theme; consistent spacing.
- Navbar: Events, Dashboard, Organizer (role-aware later).
- Mobile-first (filters collapse under accordion).

## 9) Performance
- Vite build to /wwwroot/dist; cache-bust supported by stable index.js path.
- JSON handlers return only fields needed for list/detail.

## 10) Security (phase 1 scope)
- Anti-forgery disabled for prototype fetch; re-enable later with token injection.
- Next phases: auth (cookie), role checks (Attendee/Organizer/Admin).

---

If approved, I will:
1) Implement filters panel + sorting on /Events.
2) Add success/failure toasts and loading states on details and dashboard.
3) Add simple pagination to /Events (page, pageSize handlers).
4) Re-enable antiforgery with token in Razor and fetch header.
5) Add organizer list/manage table and basic edit.
