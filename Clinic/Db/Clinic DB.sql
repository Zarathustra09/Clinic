CREATE DATABASE Clinic;
GO

-- Switch to the new database context
USE Clinic;
GO

-- Create the schema if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Clinic')
    EXEC('CREATE SCHEMA Clinic');
GO

-- Table definitions (create tables in correct order)
CREATE TABLE [Clinic].[Branches] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Name] NVARCHAR(255) NOT NULL,
    [Address] NVARCHAR(500) NULL
);

CREATE TABLE [Clinic].[User] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SchoolID] NVARCHAR(255) NULL,
    [FirstName] NVARCHAR(255) NOT NULL,
    [MiddleName] NVARCHAR(255) NULL,
    [LastName] NVARCHAR(255) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [Username] NVARCHAR(255) NOT NULL,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Role] INT NOT NULL, -- Values: 0 (Student), 1 (Doctor), 2 (Campus Clinic)
    [Program] NVARCHAR(255) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Create TimeSlot table BEFORE Appointment table
CREATE TABLE [Clinic].[TimeSlot] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [DoctorId] INT NOT NULL,
    [StartTime] DATETIME2 NOT NULL,
    [EndTime] DATETIME2 NOT NULL,
    [AppointmentId] INT NULL,
    CONSTRAINT [FK_TimeSlot_Doctor] FOREIGN KEY ([DoctorId]) REFERENCES [Clinic].[User]([Id])
);

-- Create Appointment table AFTER TimeSlot table
CREATE TABLE [Clinic].[Appointment] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,    -- Patient (Student or Campus Clinic staff)
    [DoctorId] INT NOT NULL,  -- Doctor
    [BranchId] INT NOT NULL,
    [Reason] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [TimeSlotId] INT NOT NULL,
    CONSTRAINT [FK_Appointment_User] FOREIGN KEY ([UserId]) REFERENCES [Clinic].[User]([Id]),
    CONSTRAINT [FK_Appointment_Doctor] FOREIGN KEY ([DoctorId]) REFERENCES [Clinic].[User]([Id]),
    CONSTRAINT [FK_Appointment_Branches] FOREIGN KEY ([BranchId]) REFERENCES [Clinic].[Branches]([Id]),
    CONSTRAINT [FK_Appointment_TimeSlot] FOREIGN KEY ([TimeSlotId]) REFERENCES [Clinic].[TimeSlot]([Id])
);

-- Add the circular foreign key constraint AFTER both tables exist
ALTER TABLE [Clinic].[TimeSlot]
ADD CONSTRAINT [FK_TimeSlot_Appointment] FOREIGN KEY ([AppointmentId]) REFERENCES [Clinic].[Appointment]([Id]);
GO

-- Add IsApproved column to Appointment table
ALTER TABLE [Clinic].[Appointment]
    ADD [IsApproved] BIT NOT NULL DEFAULT 0
    CONSTRAINT [CK_Appointment_IsApproved] CHECK ([IsApproved] IN (0, 1));
GO

ALTER TABLE [Clinic].[User]
    ADD [ProfileImage] NVARCHAR(500) NULL;
GO