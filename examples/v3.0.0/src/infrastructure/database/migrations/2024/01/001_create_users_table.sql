-- Migration: 001_create_users_table.sql
-- Created: 2024-01-15
-- Description: Create users table with basic user information

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);
CREATE INDEX IX_Users_IsDeleted ON Users(IsDeleted);

-- Insert sample data
INSERT INTO Users (Name, Email, PasswordHash, CreatedAt, UpdatedAt) VALUES
('John Doe', 'john.doe@example.com', 'hashed_password_1', GETUTCDATE(), GETUTCDATE()),
('Jane Smith', 'jane.smith@example.com', 'hashed_password_2', GETUTCDATE(), GETUTCDATE()),
('Bob Johnson', 'bob.johnson@example.com', 'hashed_password_3', GETUTCDATE(), GETUTCDATE());
