-- =============================================
-- Add Role column to Account table
-- Run this on existing databases
-- =============================================

USE ShopDb;
GO

-- Add Role column if not exists
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Account') AND name = 'Role')
BEGIN
    ALTER TABLE Account ADD Role NVARCHAR(20) DEFAULT 'Customer';
    PRINT 'Role column added successfully!';
END
ELSE
BEGIN
    PRINT 'Role column already exists.';
END
GO

-- Set all existing accounts to 'Customer' where Role is NULL
UPDATE Account SET Role = 'Customer' WHERE Role IS NULL;
GO

-- Set the 'admin' account to 'Admin' role
UPDATE Account SET Role = 'Admin' WHERE LOWER(UserName) = 'admin';
GO

PRINT 'Role values initialized successfully!';
GO
