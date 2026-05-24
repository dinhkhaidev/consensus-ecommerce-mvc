-- =============================================
-- Furnish E-Commerce Database Schema
-- MATCHING Entity Framework Entities Exactly
-- =============================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ShopDb')
BEGIN
    ALTER DATABASE ShopDb SET OFFLINE WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE ShopDb SET ONLINE WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE ShopDb SET MULTI_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ShopDb;
END
GO

CREATE DATABASE ShopDb;
GO

USE ShopDb;
GO

-- =============================================
-- ACCOUNT TABLE (Users)
-- =============================================
CREATE TABLE Account (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(20) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Password NVARCHAR(150) NOT NULL,
    Email NVARCHAR(50) NOT NULL,
    Phone NVARCHAR(50) NOT NULL,
    Birthday DATETIME NULL,
    Sex NVARCHAR(10) NULL,
    Address NVARCHAR(255) NULL,
    Status INT DEFAULT 1,
    Role NVARCHAR(20) DEFAULT 'Customer',
    Notes NVARCHAR(150) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsEmailVerified BIT NULL,
    EmailVerificationToken NVARCHAR(100) NULL,
    EmailVerificationTokenExpiresAt DATETIME NULL,
    AvatarUrl NVARCHAR(500) NULL
);
CREATE UNIQUE INDEX IX_Account_UserName ON Account(UserName);
CREATE UNIQUE INDEX IX_Account_Email ON Account(Email);
GO

-- =============================================
-- ADDRESSES TABLE
-- =============================================
CREATE TABLE Addresses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    AddressLine NVARCHAR(255) NOT NULL,
    Ward NVARCHAR(100) NULL,
    District NVARCHAR(100) NULL,
    City NVARCHAR(100) NULL,
    PostalCode NVARCHAR(10) NULL,
    IsDefault BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Account(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Addresses_UserId ON Addresses(UserId);
GO

-- =============================================
-- CATEGORIES TABLE
-- =============================================
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- SUPPLIERS TABLE
-- =============================================
CREATE TABLE Suppliers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CompanyName NVARCHAR(150) NOT NULL,
    Phone NVARCHAR(30) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- =============================================
-- PRODUCTS TABLE
-- =============================================
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(150) NOT NULL,
    CategoryID INT NOT NULL,
    SupplierID INT NULL,
    QuantityPerUnit NVARCHAR(100) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discontinued BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (CategoryID) REFERENCES Categories(Id),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(Id) ON DELETE SET NULL
);
CREATE INDEX IX_Products_CategoryID ON Products(CategoryID);
CREATE INDEX IX_Products_SupplierID ON Products(SupplierID);
GO

-- =============================================
-- PRODUCT VARIANTS TABLE
-- =============================================
CREATE TABLE ProductVariants (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Size NVARCHAR(50) NULL,
    Color NVARCHAR(50) NULL,
    SKU NVARCHAR(100) NULL,
    PriceAdjustment DECIMAL(18,2) NULL DEFAULT 0,
    StockQuantity INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
CREATE INDEX IX_ProductVariants_ProductId ON ProductVariants(ProductId);
GO

-- =============================================
-- PRODUCT IMAGES TABLE
-- =============================================
CREATE TABLE ProductImages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    VariantId INT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    AltText NVARCHAR(255) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsMain BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_ProductImages_ProductId ON ProductImages(ProductId);
GO

-- =============================================
-- COUPONS TABLE
-- =============================================
CREATE TABLE Coupons (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) NOT NULL,
    Description NVARCHAR(255) NULL,
    Type INT NOT NULL DEFAULT 0,
    DiscountValue DECIMAL(18,2) NOT NULL DEFAULT 0,
    MinOrderAmount DECIMAL(18,2) NULL,
    MaxDiscountAmount DECIMAL(18,2) NULL,
    StartDate DATETIME NOT NULL DEFAULT GETDATE(),
    EndDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    UsageLimit INT NOT NULL DEFAULT 1,
    UsedCount INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
CREATE UNIQUE INDEX IX_Coupons_Code ON Coupons(Code);
GO

-- =============================================
-- CARTS TABLE
-- =============================================
CREATE TABLE Carts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Account(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Carts_UserId ON Carts(UserId);
GO

-- =============================================
-- CART ITEMS TABLE (with price breakdown)
-- =============================================
CREATE TABLE CartItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    VariantId INT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    VariantName NVARCHAR(100) NULL,
    ImageUrl NVARCHAR(255) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    BasePrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    PriceAdjustment DECIMAL(18,2) NOT NULL DEFAULT 0,
    PriceBreakdown NVARCHAR(255) NULL,
    Quantity INT NOT NULL DEFAULT 1,

    FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (VariantId) REFERENCES ProductVariants(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_CartItems_CartId ON CartItems(CartId);
CREATE INDEX IX_CartItems_ProductId ON CartItems(ProductId);
GO

-- =============================================
-- ORDERS TABLE
-- =============================================
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber NVARCHAR(20) NOT NULL,
    UserId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    ShippingFee DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    AddressId INT NULL,
    ShippingName NVARCHAR(100) NULL,
    ShippingPhone NVARCHAR(20) NULL,
    ShippingAddress NVARCHAR(255) NULL,
    ShippingCity NVARCHAR(100) NULL,
    ShippingDistrict NVARCHAR(100) NULL,
    ShippingWard NVARCHAR(100) NULL,
    Notes NVARCHAR(500) NULL,
    PaymentMethod INT NULL,
    PaymentStatus INT NOT NULL DEFAULT 0,
    CouponId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    ShippedAt DATETIME NULL,
    DeliveredAt DATETIME NULL,
    CancelReason NVARCHAR(500) NULL,
    CancelRequestedAt DATETIME2 NULL,
    CancelRequestedFromStatus INT NULL,
    CancelApproved BIT NULL,
    CancelAdminNote NVARCHAR(500) NULL,
    CancelReviewedAt DATETIME2 NULL,
    ReturnReason NVARCHAR(1000) NULL,
    ReturnImageUrl NVARCHAR(500) NULL,
    ReturnRequestedAt DATETIME2 NULL,
    ReturnRequestedFromStatus INT NULL,
    ReturnApproved BIT NULL,
    ReturnAdminNote NVARCHAR(500) NULL,
    ReturnReviewedAt DATETIME2 NULL,

    FOREIGN KEY (UserId) REFERENCES Account(Id) ON DELETE NO ACTION,
    FOREIGN KEY (AddressId) REFERENCES Addresses(Id) ON DELETE NO ACTION,
    FOREIGN KEY (CouponId) REFERENCES Coupons(Id) ON DELETE SET NULL
);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE UNIQUE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
GO

-- =============================================
-- ORDER ITEMS TABLE
-- =============================================
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    VariantId INT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    VariantName NVARCHAR(50) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,

    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
GO

-- =============================================
-- PAYMENTS TABLE
-- =============================================
CREATE TABLE Payments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    Method NVARCHAR(50) NOT NULL DEFAULT 'COD',
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0,
    TransactionId NVARCHAR(100) NULL,
    PaymentUrl NVARCHAR(500) NULL,
    ReturnUrl NVARCHAR(500) NULL,
    ErrorMessage NVARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL,
    ExpiresAt DATETIME2 NULL,

    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Payments_OrderId ON Payments(OrderId);
GO

-- =============================================
-- SHIPMENTS TABLE
-- =============================================
CREATE TABLE Shipments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    Carrier NVARCHAR(100) NOT NULL DEFAULT '',
    TrackingNumber NVARCHAR(100) NULL,
    Status INT NOT NULL DEFAULT 0,
    TrackingUrl NVARCHAR(500) NULL,
    LastUpdate NVARCHAR(500) NULL,
    EstimatedDelivery NVARCHAR(500) NULL,
    ActualDelivery NVARCHAR(500) NULL,
    ShippedAt DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,

    FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Shipments_OrderId ON Shipments(OrderId);
GO

-- =============================================
-- REVIEWS TABLE
-- =============================================
CREATE TABLE Reviews (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL DEFAULT 5,
    Comment NVARCHAR(500) NOT NULL,
    IsApproved BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,

    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Account(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
GO

-- =============================================
-- WISHLISTS TABLE
-- =============================================
CREATE TABLE Wishlists (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Account(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Wishlists_UserId ON Wishlists(UserId);
CREATE INDEX IX_Wishlists_ProductId ON Wishlists(ProductId);
GO

-- =============================================
-- WEB SETTINGS TABLE
-- =============================================
CREATE TABLE WebSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
CREATE UNIQUE INDEX IX_WebSettings_Key ON WebSettings(SettingKey);
GO

-- =============================================
-- EF MIGRATIONS HISTORY TABLE
-- =============================================
CREATE TABLE __EFMigrationsHistory (
    MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion NVARCHAR(32) NOT NULL
);
GO

INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES
('20260510125916_InitialCreate', '8.0.25'),
('20260510140146_AddCartItemPriceColumns', '8.0.25'),
('20260515123000_AddOrderRequestFlow', '8.0.25'),
('20260517062015_AddAvatarUrl', '8.0.25');
GO

PRINT 'Furnish database schema created successfully!';
GO
