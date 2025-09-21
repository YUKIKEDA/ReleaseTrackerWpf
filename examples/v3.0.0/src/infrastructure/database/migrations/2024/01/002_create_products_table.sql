-- Migration: 002_create_products_table.sql
-- Created: 2024-01-16
-- Description: Create products table with product information

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Price DECIMAL(10,2) NOT NULL,
    CategoryId INT NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

ALTER TABLE Products ADD CONSTRAINT FK_Products_Categories 
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id);

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_IsDeleted ON Products(IsDeleted);

-- Insert sample categories
INSERT INTO Categories (Name, Description, CreatedAt, UpdatedAt) VALUES
('Electronics', 'Electronic devices and accessories', GETUTCDATE(), GETUTCDATE()),
('Books', 'Books and educational materials', GETUTCDATE(), GETUTCDATE()),
('Clothing', 'Apparel and fashion items', GETUTCDATE(), GETUTCDATE());
