-- Products Table Creation Script
-- For Azure SQL Database: products-db
-- Run this script if dotnet ef migrations fails

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE [dbo].[Products](
        [Code] [nvarchar](50) NOT NULL,
        [Name] [nvarchar](50) NOT NULL,
        [Category] [nvarchar](max) NOT NULL,
        [Content] [nvarchar](500) NULL,
        [IsActive] [bit] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Code] ASC)
    );
    
    PRINT 'Products table created successfully';
END
ELSE
BEGIN
    PRINT 'Products table already exists';
END
GO

-- Create indexes for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Category' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_Category]
    ON [dbo].[Products] ([Category] ASC);
    PRINT 'Index IX_Products_Category created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_IsActive' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_IsActive]
    ON [dbo].[Products] ([IsActive] ASC);
    PRINT 'Index IX_Products_IsActive created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_CreatedAt' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_CreatedAt]
    ON [dbo].[Products] ([CreatedAt] ASC);
    PRINT 'Index IX_Products_CreatedAt created';
END
GO

-- Create EF Core Migrations History table (required for EF migrations)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory](
        [MigrationId] [nvarchar](150) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC)
    );
    
    -- Insert initial migration record
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20240101000000_InitialCreate', '9.0.0');
    
    PRINT '__EFMigrationsHistory table created';
END
GO

-- Verify tables created
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.name = 'Products'
ORDER BY c.column_id;

PRINT '';
PRINT 'Database setup complete!';
PRINT 'Tables created: Products, __EFMigrationsHistory';
GO

-- Insert sample data (optional - uncomment to use)
/*
INSERT INTO [dbo].[Products] ([Code], [Name], [Category], [Content], [IsActive], [CreatedAt])
VALUES 
    ('F1001', 'Organic Bananas', 'Food', 'Fresh organic bananas sourced from sustainable farms.', 1, GETUTCDATE()),
    ('F1002', 'Whole Wheat Bread', 'Food', 'Freshly baked whole wheat bread with no artificial preservatives.', 1, GETUTCDATE()),
    ('NF2001', 'Eco Dish Soap 500ml', 'NonFood', 'Biodegradable dish soap with natural lemon scent.', 1, GETUTCDATE());

SELECT * FROM Products;
*/
