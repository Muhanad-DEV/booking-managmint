-- Event Booking System Database Schema
-- SQLite Database Schema
-- Generated for COMP4701 Project

-- Enable foreign keys
PRAGMA foreign_keys = ON;

-- ============================================
-- TABLE: Users
-- ============================================
CREATE TABLE IF NOT EXISTS "Users" (
    "UserId" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "FullName" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("UserId"),
    CONSTRAINT "CK_Users_Role" CHECK ("Role" IN (0, 1, 2))
);

-- Unique Index on Email
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");

-- ============================================
-- TABLE: Events
-- ============================================
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

-- ============================================
-- TABLE: Tickets
-- ============================================
CREATE TABLE IF NOT EXISTS "Tickets" (
    "TicketId" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "EventId" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "QRCode" TEXT NOT NULL,
    "PurchasedAt" TEXT NOT NULL,
    "SeatNumber" TEXT NULL,
    "EventId1" TEXT NULL,
    CONSTRAINT "PK_Tickets" PRIMARY KEY ("TicketId"),
    CONSTRAINT "FK_Tickets_Events_EventId" FOREIGN KEY ("EventId") 
        REFERENCES "Events" ("EventId") ON DELETE CASCADE,
    CONSTRAINT "FK_Tickets_Events_EventId1" FOREIGN KEY ("EventId1") 
        REFERENCES "Events" ("EventId"),
    CONSTRAINT "FK_Tickets_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("UserId") ON DELETE CASCADE,
    CONSTRAINT "CK_Tickets_Status" CHECK ("Status" IN (0, 1, 2, 3))
);

-- Indexes for Tickets
CREATE INDEX IF NOT EXISTS "IX_Tickets_EventId" ON "Tickets" ("EventId");
CREATE INDEX IF NOT EXISTS "IX_Tickets_EventId1" ON "Tickets" ("EventId1");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Tickets_QRCode" ON "Tickets" ("QRCode");
CREATE INDEX IF NOT EXISTS "IX_Tickets_UserId" ON "Tickets" ("UserId");

-- ============================================
-- ENUM VALUES (for reference)
-- ============================================
-- UserRole Enum:
--   0 = Attendee
--   1 = Organizer
--   2 = Admin
--
-- TicketStatus Enum:
--   0 = Reserved
--   1 = Paid
--   2 = Cancelled
--   3 = CheckedIn

-- ============================================
-- SAMPLE INSERT STATEMENTS (Optional)
-- ============================================
-- Insert sample Organizer user
-- INSERT INTO "Users" ("UserId", "Email", "PasswordHash", "FullName", "Role")
-- VALUES ('00000000-0000-0000-0000-000000000001', 'org@example.com', '<hashed>', 'Organizer One', 1);

-- Insert sample Attendee user
-- INSERT INTO "Users" ("UserId", "Email", "PasswordHash", "FullName", "Role")
-- VALUES ('00000000-0000-0000-0000-000000000002', 'user@example.com', '<hashed>', 'Attendee One', 0);

-- Insert sample Event
-- INSERT INTO "Events" ("EventId", "Title", "Description", "DateTime", "Venue", "Capacity", "RemainingSeats", "Price", "Category")
-- VALUES ('00000000-0000-0000-0000-000000000010', 'Tech Talk', 'Intro to ASP.NET Core', datetime('now', '+7 days'), 'Auditorium A', 50, 50, '0.0', 'Tech');

-- ============================================
-- VERIFICATION QUERIES
-- ============================================
-- Check all tables exist
-- SELECT name FROM sqlite_master WHERE type='table' AND name IN ('Users', 'Events', 'Tickets');

-- Check all indexes
-- SELECT name FROM sqlite_master WHERE type='index';

-- Check foreign key constraints
-- PRAGMA foreign_key_list('Tickets');

