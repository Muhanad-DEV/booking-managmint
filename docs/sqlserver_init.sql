IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        PasswordHash NVARCHAR(200) NOT NULL,
        Role INT NOT NULL,
        CONSTRAINT PK_Users PRIMARY KEY (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Role CHECK (Role BETWEEN 0 AND 2)
    );
END;

IF OBJECT_ID('dbo.Logins', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Logins (
        LoginId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        Username NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        Phone NVARCHAR(30) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Logins PRIMARY KEY (LoginId),
        CONSTRAINT FK_Logins_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        CONSTRAINT UQ_Logins_Username UNIQUE (Username),
        CONSTRAINT UQ_Logins_Email UNIQUE (Email)
    );
END;

IF OBJECT_ID('dbo.Events', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Events (
        EventId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        Title NVARCHAR(120) NOT NULL,
        Description NVARCHAR(400) NOT NULL,
        DateTime DATETIME2 NOT NULL,
        Venue NVARCHAR(200) NOT NULL,
        Capacity INT NOT NULL CONSTRAINT DF_Events_Capacity DEFAULT 0,
        RemainingSeats INT NOT NULL CONSTRAINT DF_Events_RemainingSeats DEFAULT 0,
        Price DECIMAL(10,2) NOT NULL CONSTRAINT DF_Events_Price DEFAULT 0,
        Category NVARCHAR(80) NOT NULL,
        CONSTRAINT PK_Events PRIMARY KEY (EventId),
        CONSTRAINT CK_Events_Capacity CHECK (Capacity >= 0),
        CONSTRAINT CK_Events_Remaining CHECK (RemainingSeats >= 0),
        CONSTRAINT CK_Events_Price CHECK (Price >= 0)
    );
END;

IF OBJECT_ID('dbo.Tickets', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tickets (
        TicketId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        EventId UNIQUEIDENTIFIER NOT NULL,
        Status INT NOT NULL CONSTRAINT DF_Tickets_Status DEFAULT 0,
        QRCode NVARCHAR(100) NOT NULL,
        PurchasedAt DATETIME2 NOT NULL CONSTRAINT DF_Tickets_PurchasedAt DEFAULT SYSUTCDATETIME(),
        SeatNumber NVARCHAR(20) NULL,
        CONSTRAINT PK_Tickets PRIMARY KEY (TicketId),
        CONSTRAINT UQ_Tickets_QR UNIQUE (QRCode),
        CONSTRAINT FK_Tickets_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_Tickets_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(EventId) ON DELETE CASCADE,
        CONSTRAINT CK_Tickets_Status CHECK (Status BETWEEN 0 AND 3)
    );
END;

IF OBJECT_ID('dbo.vw_UserTickets', 'V') IS NOT NULL
    DROP VIEW dbo.vw_UserTickets;

IF OBJECT_ID('dbo.vw_UserTickets', 'V') IS NULL
BEGIN
    EXEC('CREATE VIEW dbo.vw_UserTickets
    AS
    SELECT
        u.UserId,
        u.FullName,
        u.Email,
        t.TicketId,
        t.Status,
        t.PurchasedAt,
        e.EventId,
        e.Title,
        e.DateTime AS EventDateTime,
        e.Venue,
        e.Category
    FROM dbo.Tickets t
    JOIN dbo.Users u ON u.UserId = t.UserId
    JOIN dbo.Events e ON e.EventId = t.EventId');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (UserId, FullName, Email, PasswordHash, Role)
    VALUES
    ('00000000-0000-0000-0000-000000000001', 'Organizer One', 'org@example.com', 'hashed-org', 1),
    ('00000000-0000-0000-0000-000000000002', 'Attendee One', 'attendee@example.com', 'hashed-att', 0),
    ('00000000-0000-0000-0000-000000000003', 'Admin One', 'admin@example.com', 'hashed-admin', 2);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Logins)
BEGIN
    INSERT INTO dbo.Logins (LoginId, UserId, Username, PasswordHash, Email, Phone)
    VALUES
    (NEWID(), '00000000-0000-0000-0000-000000000001', 'organizer', 'hashed-org', 'org@example.com', '+96898888888'),
    (NEWID(), '00000000-0000-0000-0000-000000000002', 'attendee', 'hashed-att', 'attendee@example.com', '+96898888888'),
    (NEWID(), '00000000-0000-0000-0000-000000000003', 'admin', 'hashed-admin', 'admin@example.com', '+96898888888');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Events)
BEGIN
    INSERT INTO dbo.Events (EventId, Title, Description, DateTime, Venue, Capacity, RemainingSeats, Price, Category)
    VALUES
    ('00000000-0000-0000-0000-000000000010', 'Tech Talk', 'Intro to ASP.NET Core', DATEADD(day, 7, SYSUTCDATETIME()), 'Auditorium A', 50, 50, 25.00, 'Technology'),
    ('00000000-0000-0000-0000-000000000011', 'React Workshop', 'React basics with hands-on labs', DATEADD(day, 14, SYSUTCDATETIME()), 'Lab 1', 30, 30, 50.00, 'Workshop'),
    ('00000000-0000-0000-0000-000000000012', 'Music Festival', 'Annual music festival featuring local artists', DATEADD(day, 21, SYSUTCDATETIME()), 'Concert Hall', 200, 200, 75.00, 'Music'),
    ('00000000-0000-0000-0000-000000000013', 'Business Conference', 'Networking and business development conference', DATEADD(day, 30, SYSUTCDATETIME()), 'Convention Center', 150, 150, 100.00, 'Business'),
    ('00000000-0000-0000-0000-000000000014', 'Sports Tournament', 'Local basketball championship tournament', DATEADD(day, 10, SYSUTCDATETIME()), 'Sports Arena', 500, 500, 15.00, 'Sports'),
    ('00000000-0000-0000-0000-000000000015', 'Art Exhibition', 'Contemporary art exhibition featuring local artists', DATEADD(day, 5, SYSUTCDATETIME()), 'Art Gallery', 80, 80, 20.00, 'Arts');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Tickets)
BEGIN
    INSERT INTO dbo.Tickets (TicketId, UserId, EventId, Status, QRCode, PurchasedAt, SeatNumber)
    VALUES
    (NEWID(), '00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000010', 1, 'QR-1001', DATEADD(day, -1, SYSUTCDATETIME()), NULL),
    (NEWID(), '00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000011', 0, 'QR-1002', DATEADD(hour, -12, SYSUTCDATETIME()), NULL);
END;

