USE Clinic;
GO

-- Check if data already exists
IF EXISTS (SELECT 1 FROM [Clinic].[User])
BEGIN
    PRINT 'Database already contains data. Skipping seeding process.';
    PRINT 'To re-seed, please clear the database first.';
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
    ('Taft Campus', '2544 Taft Avenue, Malate, Manila'),
    ('Taft (dental)', '2544 Taft Avenue, Malate, Manila - Dental Wing'),
    ('AKIC Campus', 'Sen. Gil Puyat Avenue, Makati City'),
    ('D+A Campus', 'Dasmariñas, Cavite'),
    ('Atrium Campus', 'Sucat, Muntinlupa City');

PRINT 'Seeded 5 branches.';

-- Seed Users
PRINT 'Seeding users...';

-- Campus Clinic Staff (Role 2) - Password: admin123
INSERT INTO [Clinic].[User] ([SchoolID], [FirstName], [MiddleName], [LastName], [Email], [Username], [PasswordHash], [Role], [Program], [CreatedAt], [UpdatedAt]) VALUES
    ('STAFF001', 'Admin', '', 'User', 'admin@clinic.university.edu', 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2, 'Administration', GETUTCDATE(), GETUTCDATE()),
    ('STAFF002', 'Sarah', 'M', 'Johnson', 'sarah.johnson@clinic.university.edu', 'sjohnson', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2, 'Nursing', GETUTCDATE(), GETUTCDATE());

-- Doctors (Role 1) - Password: doctor123
INSERT INTO [Clinic].[User] ([SchoolID], [FirstName], [MiddleName], [LastName], [Email], [Username], [PasswordHash], [Role], [Program], [CreatedAt], [UpdatedAt]) VALUES
    ('DOC001', 'Michael', 'A', 'Smith', 'michael.smith@university.edu', 'msmith', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, 'General Medicine', GETUTCDATE(), GETUTCDATE()),
    ('DOC002', 'Emily', 'R', 'Brown', 'emily.brown@university.edu', 'ebrown', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, 'Internal Medicine', GETUTCDATE(), GETUTCDATE()),
    ('DOC003', 'James', 'L', 'Wilson', 'james.wilson@university.edu', 'jwilson', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, 'Pediatrics', GETUTCDATE(), GETUTCDATE()),
    ('DOC004', 'Lisa', 'K', 'Davis', 'lisa.davis@university.edu', 'ldavis', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, 'Dermatology', GETUTCDATE(), GETUTCDATE()),
    ('DOC005', 'Robert', 'J', 'Anderson', 'robert.anderson@university.edu', 'randerson', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 1, 'Cardiology', GETUTCDATE(), GETUTCDATE());

-- Students (Role 0) - Password: student123
INSERT INTO [Clinic].[User] ([SchoolID], [FirstName], [MiddleName], [LastName], [Email], [Username], [PasswordHash], [Role], [Program], [CreatedAt], [UpdatedAt]) VALUES
    ('2023001', 'John', 'A', 'Doe', 'john.doe@student.university.edu', 'jdoe', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Computer Science', GETUTCDATE(), GETUTCDATE()),
    ('2023002', 'Jane', 'B', 'Smith', 'jane.smith@student.university.edu', 'jsmith', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Engineering', GETUTCDATE(), GETUTCDATE()),
    ('2023003', 'Mike', 'C', 'Johnson', 'mike.johnson@student.university.edu', 'mjohnson', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Business', GETUTCDATE(), GETUTCDATE()),
    ('2023004', 'Emma', 'D', 'Williams', 'emma.williams@student.university.edu', 'ewilliams', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Psychology', GETUTCDATE(), GETUTCDATE()),
    ('2023005', 'David', 'E', 'Brown', 'david.brown@student.university.edu', 'dbrown', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Biology', GETUTCDATE(), GETUTCDATE()),
    ('2023006', 'Sarah', 'F', 'Garcia', 'sarah.garcia@student.university.edu', 'sgarcia', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Chemistry', GETUTCDATE(), GETUTCDATE()),
    ('2023007', 'Alex', 'G', 'Martinez', 'alex.martinez@student.university.edu', 'amartinez', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Physics', GETUTCDATE(), GETUTCDATE()),
    ('2023008', 'Olivia', 'H', 'Taylor', 'olivia.taylor@student.university.edu', 'otaylor', 'BPiZbadjt6lpsQKO4wB1aerzpjVIbdqyEdUSyFud+Ps=', 0, 'Mathematics', GETUTCDATE(), GETUTCDATE());

PRINT 'Seeded 15 users (2 clinic staff, 5 doctors, 8 students).';

-- Seed Time Slots based on specific campus schedules
PRINT 'Seeding time slots...';

DECLARE @CurrentDate DATE = CAST(DATEADD(DAY, 1, GETDATE()) AS DATE);
DECLARE @EndDate DATE = DATEADD(DAY, 30, @CurrentDate);
DECLARE @DoctorId INT;
DECLARE @SlotStart DATETIME2;
DECLARE @SlotEnd DATETIME2;
DECLARE @CurrentDateTime DATETIME2;

-- Get doctor IDs
DECLARE doctor_cursor CURSOR FOR
SELECT Id FROM [Clinic].[User] WHERE Role = 1;

WHILE @CurrentDate <= @EndDate
BEGIN
    -- Skip weekends (Saturday = 7, Sunday = 1 in SQL Server)
    IF DATEPART(WEEKDAY, @CurrentDate) NOT IN (1, 7)
BEGIN
        SET @CurrentDateTime = CAST(@CurrentDate AS DATETIME2);

OPEN doctor_cursor;
FETCH NEXT FROM doctor_cursor INTO @DoctorId;

WHILE @@FETCH_STATUS = 0
BEGIN
            -- Taft Campus (ID: 1) - 8:00 AM to 12:00 PM & 1:00 PM to 5:00 PM (1-hour slots)
            -- Morning slots: 8:00-9:00, 9:00-10:00, 10:00-11:00, 11:00-12:00
            DECLARE @Hour INT = 8;
            WHILE @Hour < 12
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            -- Afternoon slots: 1:00-2:00, 2:00-3:00, 3:00-4:00, 4:00-5:00
            SET @Hour = 13;
            WHILE @Hour < 17
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            -- Taft (dental) (ID: 2) - 9:00 AM to 1:00 PM (1-hour slots)
            -- Morning slots: 9:00-10:00, 10:00-11:00, 11:00-12:00, 12:00-1:00
            SET @Hour = 9;
            WHILE @Hour < 13
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            -- AKIC Campus (ID: 3) - 8:00 AM to 12:00 PM & 1:00 PM to 5:00 PM (1-hour slots)
            -- Same as Taft Campus
            SET @Hour = 8;
            WHILE @Hour < 12
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            SET @Hour = 13;
            WHILE @Hour < 17
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            -- D+A Campus (ID: 4) - 8:00 AM to 12:00 PM & 1:00 PM to 5:00 PM (1-hour slots)
            -- Same as Taft Campus
            SET @Hour = 8;
            WHILE @Hour < 12
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            SET @Hour = 13;
            WHILE @Hour < 17
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            -- Atrium Campus (ID: 5) - 8:00 AM to 12:00 PM & 1:00 PM to 5:00 PM (1-hour slots)
            -- Same as Taft Campus
            SET @Hour = 8;
            WHILE @Hour < 12
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;
            
            SET @Hour = 13;
            WHILE @Hour < 17
BEGIN
                SET @SlotStart = DATEADD(HOUR, @Hour, @CurrentDateTime);
                SET @SlotEnd = DATEADD(HOUR, @Hour + 1, @CurrentDateTime);
INSERT INTO [Clinic].[TimeSlot] ([DoctorId], [StartTime], [EndTime]) VALUES (@DoctorId, @SlotStart, @SlotEnd);
SET @Hour = @Hour + 1;
END;

FETCH NEXT FROM doctor_cursor INTO @DoctorId;
END;

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
       (ABS(CHECKSUM(NEWID())) % 5) + 1 as BranchId,
       CASE (ABS(CHECKSUM(NEWID())) % 13)
           WHEN 0 THEN 'General consultation'
           WHEN 1 THEN 'Follow-up visit'
           WHEN 2 THEN 'Routine checkup'
           WHEN 3 THEN 'Vaccination'
           WHEN 4 THEN 'Medical certificate'
           WHEN 5 THEN 'Blood pressure monitoring'
           WHEN 6 THEN 'Minor injury treatment'
           WHEN 7 THEN 'Prescription renewal'
           WHEN 8 THEN 'Fever and flu symptoms'
           WHEN 9 THEN 'Headache consultation'
           WHEN 10 THEN 'Allergy consultation'
           WHEN 11 THEN 'Stomach pain'
           WHEN 12 THEN 'Annual physical exam'
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
PRINT '=== CAMPUS SCHEDULES ===';
PRINT 'Taft Campus: 8:00 AM-12:00 PM & 1:00 PM-5:00 PM (Peak: 9:00-11:00 AM & 1:00-3:00 PM)';
PRINT 'Taft (dental): 9:00 AM-1:00 PM (Peak: 9:00-11:00 AM)';
PRINT 'AKIC Campus: 8:00 AM-12:00 PM & 1:00 PM-5:00 PM (Peak: 9:00-11:00 AM & 1:00-3:00 PM)';
PRINT 'D+A Campus: 8:00 AM-12:00 PM & 1:00 PM-5:00 PM (Peak: 9:00-11:00 AM & 1:00-3:00 PM)';
PRINT 'Atrium Campus: 8:00 AM-12:00 PM & 1:00 PM-5:00 PM (Peak: 9:00-11:00 AM & 1:00-3:00 PM)';
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