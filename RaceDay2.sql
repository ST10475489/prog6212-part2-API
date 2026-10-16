--CREATE DATABASE RaceDay2;

--CREATE TABLE dbo.Users (
--    UserId INT IDENTITY(1,1) PRIMARY KEY,
--    Role VARCHAR(20) NOT NULL,
--    FullName VARCHAR(100) NOT NULL,
--    Email VARCHAR(150) NOT NULL UNIQUE,
--    PasswordHash VARCHAR(255) NOT NULL,
--    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
--);

--CREATE TABLE dbo.Events (
--    EventId INT IDENTITY(1,1) PRIMARY KEY,
--    OrganiserId INT NOT NULL,
--    EventName VARCHAR(150) NOT NULL,
--    EventDate DATE NOT NULL,
--    Location VARCHAR(150) NOT NULL,
--    Description VARCHAR(1000),
--    FOREIGN KEY (OrganiserId) REFERENCES Users(UserId)
--);

--CREATE TABLE dbo.Categories (
--    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
--    EventId INT NOT NULL,
--    CategoryName VARCHAR(100) NOT NULL,
--    DistanceKm DECIMAL(5,2) NOT NULL,
--    MaxParticipants INT NOT NULL DEFAULT 100,
--    FOREIGN KEY (EventId) REFERENCES Events(EventId)
--);

--CREATE TABLE dbo.Routes (
--    RouteId INT IDENTITY(1,1) PRIMARY KEY,
--    EventId INT NOT NULL,
--    RouteName VARCHAR(100) NOT NULL,
--    StartPoint VARCHAR(150) NOT NULL,
--    EndPoint VARCHAR(150) NOT NULL,
--    MapUrl VARCHAR(255),
--    FOREIGN KEY (EventId) REFERENCES Events(EventId)
--);

--CREATE TABLE dbo.Enrolments (
--    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
--    ParticipantId INT NOT NULL,
--    CategoryId INT NOT NULL,
--    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
--    Status VARCHAR(20) NOT NULL DEFAULT 'Confirmed',
--    FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
--    FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
--);

--CREATE TABLE dbo.Results (
--    ResultId INT IDENTITY(1,1) PRIMARY KEY,
--    EnrolmentId INT NOT NULL UNIQUE,
--    FinishTime VARCHAR(20) NOT NULL,
--    Position INT,
--    FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId)
--);

--INSERT INTO Users (Role, FullName, Email, PasswordHash) VALUES
--('Organiser', 'Thandiwe Mokoena', 'thandiwe@raceday.co.za', 'HASH_PLACEHOLDER_1'),
--('Organiser', 'Pieter van Wyk', 'pieter@raceday.co.za', 'HASH_PLACEHOLDER_2'),
--('Participant', 'Lerato Dube', 'lerato@example.com', 'HASH_PLACEHOLDER_3'),
--('Participant', 'Sarah Naidoo', 'sarah@example.com', 'HASH_PLACEHOLDER_4');

--INSERT INTO Events (OrganiserId, EventName, EventDate, Location, Description) VALUES
--(1, 'Pretoria Park Run Challenge', '2026-11-14', 'Pretoria, Gauteng', 'A family-friendly road running event through Pretoria parks.'),
--(1, 'Soweto Community Cycle', '2026-11-21', 'Soweto, Gauteng', 'A charity cycling event supporting local youth sports programmes.'),
--(2, 'Durban Coastal Marathon', '2026-12-05', 'Durban, KwaZulu-Natal', 'A scenic marathon and half marathon along the Durban coastline.');

--INSERT INTO Categories (EventId, CategoryName, DistanceKm, MaxParticipants) VALUES
--(1, '5km Fun Run', 5.00, 200),
--(1, '10km Road Race', 10.00, 150),
--(2, '20km Charity Ride', 20.00, 100),
--(3, 'Half Marathon', 21.10, 300);

--INSERT INTO Enrolments (ParticipantId, CategoryId, Status) VALUES
--(3, 1, 'Confirmed'),
--(3, 3, 'Confirmed'),
--(4, 2, 'Confirmed'),
--(4, 4, 'Confirmed');

--INSERT INTO Results (EnrolmentId, FinishTime, Position) VALUES
--(1, '00:28:14', 12),
--(3, '01:05:47', 8);

