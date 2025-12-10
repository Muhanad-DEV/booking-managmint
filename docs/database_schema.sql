PRAGMA foreign_keys = ON;

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

CREATE INDEX IF NOT EXISTS "IX_Tickets_EventId" ON "Tickets" ("EventId");
CREATE INDEX IF NOT EXISTS "IX_Tickets_EventId1" ON "Tickets" ("EventId1");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Tickets_QRCode" ON "Tickets" ("QRCode");
CREATE INDEX IF NOT EXISTS "IX_Tickets_UserId" ON "Tickets" ("UserId");

