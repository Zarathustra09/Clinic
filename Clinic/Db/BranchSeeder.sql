USE Clinic;
GO

-- Seed Branches
PRINT 'Seeding branches...';

INSERT INTO [Clinic].[Branches] ([Name], [Address]) VALUES
    ('Taft Campus', '2544 Taft Avenue, Malate, Manila'),
    ('Taft (dental)', '2544 Taft Avenue, Malate, Manila - Dental Wing'),
    ('AKIC Campus', 'Corner Estrada Street and Arellano Avenue, Malate, Manila'),
    ('D+A Campus', 'Pablo Ocampo Street, Malate, Manila'),
    ('Atrium Campus', '1040 Arellano Avenue, corner C. Ayala Street (Don Pedro Street), Malate, Manila');

PRINT 'Seeded 5 branches.';
GO
