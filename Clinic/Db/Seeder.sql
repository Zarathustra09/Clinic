USE Clinic;
GO

-- Check if data already exists
IF EXISTS (SELECT 1 FROM [Clinic].[User])
BEGIN
    PRINT 'Database already contains data. Skipping seeding process.';
    RETURN;
END

PRINT 'Starting database seeding...';

-- Disable foreign key constraints temporarily for easier seeding
ALTER TABLE [Clinic].[TimeSlot] NOCHECK CONSTRAINT [FK_TimeSlot_Appointment];
ALTER TABLE [Clinic].[Appointment] NOCHECK CONSTRAINT ALL;
ALTER TABLE [Clinic].[TimeSlot] NOCHECK CONSTRAINT [FK_TimeSlot_Doctor];

-- Seed Branches
PRINT 'Seeding branches...';
INSERT INTO [Clinic].[Branches] ([Name], [Address]) VALUES
('Main Campus Clinic', 'University Main Campus, Building A, Ground Floor'),
('North Campus Clinic', 'North Campus, Medical Center, 2nd Floor'),
('South Campus Clinic', 'South Campus, Health Services Building'),
('Downtown Clinic', '123 Downtown Medical Plaza, Suite 200'),
('Community Health Center', '456 Community Drive, Medical Wing');

PRINT 'Seeded 5 branches.';

-- Seed Users
PRINT 'Seeding users...';

-- Campus Clinic Staff (Role 2)
INSERT INTO [Clinic].[User] ([SchoolID], [FirstName], [MiddleName], [LastName], [Email], [Username], [PasswordHash], [Role], [Program], [CreatedAt], [UpdatedAt]) VALUES
(NULL, 'Admin', NULL, 'User', 'admin@university.edu', 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2, NULL, GETUTCDATE(), GETUTCDATE()),
(NULL, 'Sarah', 'M', 'Johnson', 's.johnson@university.edu', 'sjohnson', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2, NULL, GETUTCDATE(), GETUTCDATE());

-- Doctors (Role 1)
INSERT INTO [Clinic].[User] ([SchoolID], [FirstName], [MiddleName], [LastName], [Email], [Username], [PasswordHash], [Role], [Program], [CreatedAt], [UpdatedAt]) VALUES
(NULL, 'Dr. Michael', 'R', 'Smith', 'm.smith@university.edu', 'msmith', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, NULL, GETUTCDATE(), GETUTCDATE()),
(NULL, 'Dr. Emily', 'K', 'Brown', 'e.brown@university.edu', 'ebrown', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, NULL, GETUTCDATE(), GETUTCDATE()),
(NULL, 'Dr. James', 'T', 'Wilson', 'j.wilson@university.edu', 'jwilson', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, NULL, GETUTCDATE(), GETUTCDATE()),
(NULL, 'Dr. Lisa', 'A', 'Davis', 'l.davis@university.edu', 'ldavis', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, NULL, GETUTCDATE(), GETUTCDATE()),
(NULL, 'Dr. Robert', 'J', 'Anderson', 'r.anderson@university.edu', 'randerson', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, NULL, GETUTCDATE(), GETUTCDATE());

-- Students (Role 0)
INSERT INTO [Clinic].[User] ([SchoolID], [FirstName], [MiddleName], [LastName], [Email], [Username], [PasswordHash], [Role], [Program], [CreatedAt], [UpdatedAt]) VALUES
('2023001', 'John', 'A', 'Doe', 'john.doe@student.university.edu', 'jdoe', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Computer Science', GETUTCDATE(), GETUTCDATE()),
('2023002', 'Jane', 'B', 'Smith', 'jane.smith@student.university.edu', 'jsmith', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Engineering', GETUTCDATE(), GETUTCDATE()),
('2023003', 'Mike', 'C', 'Johnson', 'mike.johnson@student.university.edu', 'mjohnson', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Business', GETUTCDATE(), GETUTCDATE()),
('2023004', 'Emma', 'D', 'Williams', 'emma.williams@student.university.edu', 'ewilliams', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Psychology', GETUTCDATE(), GETUTCDATE()),
('2023005', 'David', 'E', 'Brown', 'david.brown@student.university.edu', 'dbrown', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Biology', GETUTCDATE(), GETUTCDATE()),
('2023006', 'Sofia', 'F', 'Garcia', 'sofia.garcia@student.university.edu', 'sgarcia', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Chemistry', GETUTCDATE(), GETUTCDATE()),
('2023007', 'Alex', 'G', 'Martinez', 'alex.martinez@student.university.edu', 'amartinez', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Physics', GETUTCDATE(), GETUTCDATE()),
('2023008', 'Olivia', 'H', 'Taylor', 'olivia.taylor@student.university.edu', 'otaylor', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Mathematics', GETUTCDATE(), GETUTCDATE());

PRINT 'Seeded 15 users (2 clinic staff, 5 doctors, 8 students).';

-- Seed Time Slots for the next 30 days (weekdays only)
PRINT 'Seeding time slots...';

DECLARE @CurrentDate DATE = CAST(DATEADD(DAY, 1, GETDATE()) AS DATE);
DECLARE @EndDate DATE = DATEADD(DAY, 30, @CurrentDate);
DECLARE @DoctorId INT;
DECLARE @Hour INT;
DECLARE @SlotStart DATETIME2;
DECLARE @SlotEnd DATETIME2;
DECLARE @CurrentDateTime DATETIME2; -- Add this variable to work with DATETIME2

-- Get doctor IDs
DECLARE doctor_cursor CURSOR FOR
SELECT Id FROM [Clinic].[User] WHERE Role = 1;

WHILE @CurrentDate <= @EndDate
BEGIN
    -- Skip weekends (Saturday = 7, Sunday = 1 in SQL Server)
    IF DATEPART(WEEKDAY, @CurrentDate) NOT IN (1, 7)
    BEGIN
        -- Convert DATE to DATETIME2 for proper datetime arithmetic
        SET @CurrentDateTime = CAST(@CurrentDate AS DATETIME2);
        
        OPEN doctor_cursor;
        FETCH NEXT FROM doctor_cursor INTO @DoctorId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Morning slots: 9:00 AM - 12:00 PM (30-minute slots)
            SET @Hour = 9;
            WHILE @Hour < 12
            BEGIN
                -- Create datetime by adding hours to the base datetime
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(MINUTE, 30, @SlotStart);

                INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime], [AppointmentId])
                VALUES (@DoctorId, @SlotStart, @SlotEnd, NULL);

                -- Second 30-minute slot in the hour
                SET @SlotStart = DATEADD(MINUTE, 30, @SlotStart);
                SET @SlotEnd = DATEADD(MINUTE, 30, @SlotStart);

                INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime], [AppointmentId])
                VALUES (@DoctorId, @SlotStart, @SlotEnd, NULL);

                SET @Hour = @Hour + 1;
            END

            -- Afternoon slots: 1:00 PM - 5:00 PM (30-minute slots)
            SET @Hour = 13; -- 1 PM in 24-hour format
            WHILE @Hour < 17 -- 5 PM in 24-hour format
            BEGIN
                -- Create datetime by adding hours to the base datetime
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(MINUTE, 30, @SlotStart);

                INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime], [AppointmentId])
                VALUES (@DoctorId, @SlotStart, @SlotEnd, NULL);

                -- Second 30-minute slot in the hour
                SET @SlotStart = DATEADD(MINUTE, 30, @SlotStart);
                SET @SlotEnd = DATEADD(MINUTE, 30, @SlotStart);

                INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime], [AppointmentId])
                VALUES (@DoctorId, @SlotStart, @SlotEnd, NULL);

                SET @Hour = @Hour + 1;
            END

            FETCH NEXT FROM doctor_cursor INTO @DoctorId;
        END

        CLOSE doctor_cursor;
    END

    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

DEALLOCATE doctor_cursor;

DECLARE @TimeSlotCount INT;
SELECT @TimeSlotCount = COUNT(*) FROM [Clinic].[TimeSlot];
PRINT 'Seeded ' + CAST(@TimeSlotCount AS VARCHAR) + ' time slots.';

-- Seed Sample Appointments
PRINT 'Seeding appointments...';

INSERT INTO [Clinic].[Appointment] ([UserId], [DoctorId], [BranchId], [Reason], [TimeSlotId], [CreatedAt])
SELECT TOP 15
    Students.Id as UserId,
    ts.DoctorId,
    ((ABS(CHECKSUM(NEWID())) % 5) + 1) as BranchId,
    CASE (ABS(CHECKSUM(NEWID())) % 13)
        WHEN 0 THEN 'General Check-up'
        WHEN 1 THEN 'Follow-up Visit'
        WHEN 2 THEN 'Flu Symptoms'
        WHEN 3 THEN 'Physical Examination'
        WHEN 4 THEN 'Vaccination'
        WHEN 5 THEN 'Blood Pressure Check'
        WHEN 6 THEN 'Consultation'
        WHEN 7 THEN 'Allergy Treatment'
        WHEN 8 THEN 'Prescription Renewal'
        WHEN 9 THEN 'Lab Results Review'
        WHEN 10 THEN 'Headache Treatment'
        WHEN 11 THEN 'Sports Physical'
        WHEN 12 THEN 'Wellness Check'
        ELSE 'General Consultation'
    END as Reason,
    ts.Id as TimeSlotId,
    GETUTCDATE()
FROM [Clinic].[TimeSlot] ts
CROSS JOIN (
    SELECT Id FROM [Clinic].[User] WHERE Role = 0
) Students
WHERE ts.AppointmentId IS NULL
    AND ts.StartTime > GETDATE()
ORDER BY NEWID();

-- Update TimeSlots to reference the appointments
UPDATE ts
SET AppointmentId = a.Id
FROM [Clinic].[TimeSlot] ts
INNER JOIN [Clinic].[Appointment] a ON ts.Id = a.TimeSlotId;

DECLARE @AppointmentCount INT;
SELECT @AppointmentCount = COUNT(*) FROM [Clinic].[Appointment];
PRINT 'Seeded ' + CAST(@AppointmentCount AS VARCHAR) + ' appointments.';

-- Re-enable foreign key constraints
ALTER TABLE [Clinic].[TimeSlot] CHECK CONSTRAINT [FK_TimeSlot_Appointment];
ALTER TABLE [Clinic].[Appointment] CHECK CONSTRAINT ALL;
ALTER TABLE [Clinic].[TimeSlot] CHECK CONSTRAINT [FK_TimeSlot_Doctor];

PRINT 'Database seeding completed successfully!';
PRINT '';
PRINT '=== TEST ACCOUNTS ===';
PRINT 'Campus Clinic Staff:';
PRINT '  Username: admin, Password: admin123';
PRINT '  Username: sjohnson, Password: admin123';
PRINT '';
PRINT 'Doctors:';
PRINT '  Username: msmith, Password: doctor123';
PRINT '  Username: ebrown, Password: doctor123';
PRINT '  Username: jwilson, Password: doctor123';
PRINT '  Username: ldavis, Password: doctor123';
PRINT '  Username: randerson, Password: doctor123';
PRINT '';
PRINT 'Students:';
PRINT '  Username: jdoe, Password: student123';
PRINT '  Username: jsmith, Password: student123';
PRINT '  Username: mjohnson, Password: student123';
PRINT '  Username: ewilliams, Password: student123';
PRINT '  Username: dbrown, Password: student123';
PRINT '  Username: sgarcia, Password: student123';
PRINT '  Username: amartinez, Password: student123';
PRINT '  Username: otaylor, Password: student123';
PRINT '';
PRINT '=== SUMMARY ===';
SELECT
    'Branches' as TableName, COUNT(*) as RecordCount FROM [Clinic].[Branches]
UNION ALL
SELECT
    'Users', COUNT(*) FROM [Clinic].[User]
UNION ALL
SELECT
    'TimeSlots', COUNT(*) FROM [Clinic].[TimeSlot]
UNION ALL
SELECT
    'Appointments', COUNT(*) FROM [Clinic].[Appointment];
GO