-- =============================================
-- Furnish E-Commerce Seed Data
-- Matching EF Models Exactly
-- =============================================

USE ShopDb;
GO

-- =========================
-- FIX MISSING COLUMNS (Schema migration)
-- =========================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Account') AND name = 'CreatedAt')
    ALTER TABLE Account ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

UPDATE Account SET CreatedAt = GETDATE() WHERE CreatedAt IS NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Categories') AND name = 'CreatedAt')
    ALTER TABLE Categories ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Products') AND name = 'CreatedAt')
    ALTER TABLE Products ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Addresses') AND name = 'CreatedAt')
    ALTER TABLE Addresses ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Suppliers') AND name = 'CreatedAt')
    ALTER TABLE Suppliers ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('ProductVariants') AND name = 'CreatedAt')
    ALTER TABLE ProductVariants ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('ProductVariants') AND name = 'IsActive')
    ALTER TABLE ProductVariants ADD IsActive BIT NOT NULL DEFAULT 1;
GO

IF EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('ProductVariants') AND name = 'SKU')
    ALTER TABLE ProductVariants ALTER COLUMN SKU NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('ProductImages') AND name = 'CreatedAt')
    ALTER TABLE ProductImages ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('ProductImages') AND name = 'DisplayOrder')
    ALTER TABLE ProductImages ADD DisplayOrder INT NOT NULL DEFAULT 0;
GO

IF EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('ProductImages') AND name = 'ImageUrl')
BEGIN
    UPDATE ProductImages SET ImageUrl = '' WHERE ImageUrl IS NULL;
    ALTER TABLE ProductImages ALTER COLUMN ImageUrl NVARCHAR(500) NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Reviews') AND name = 'CreatedAt')
    ALTER TABLE Reviews ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Wishlists') AND name = 'CreatedAt')
    ALTER TABLE Wishlists ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Coupons') AND name = 'CreatedAt')
    ALTER TABLE Coupons ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Payments') AND name = 'CreatedAt')
    ALTER TABLE Payments ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Shipments') AND name = 'CreatedAt')
    ALTER TABLE Shipments ADD CreatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Shipments') AND name = 'UpdatedAt')
    ALTER TABLE Shipments ADD UpdatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Payments') AND name = 'CompletedAt')
    ALTER TABLE Payments ADD CompletedAt DATETIME DEFAULT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Payments') AND name = 'ExpiresAt')
    ALTER TABLE Payments ADD ExpiresAt DATETIME DEFAULT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Payments') AND name = 'UpdatedAt')
    ALTER TABLE Payments ADD UpdatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'UpdatedAt')
    ALTER TABLE Orders ADD UpdatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ShippedAt')
    ALTER TABLE Orders ADD ShippedAt DATETIME DEFAULT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'DeliveredAt')
    ALTER TABLE Orders ADD DeliveredAt DATETIME DEFAULT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'CancelReason')
    ALTER TABLE Orders ADD CancelReason NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'CancelRequestedAt')
    ALTER TABLE Orders ADD CancelRequestedAt DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'CancelRequestedFromStatus')
    ALTER TABLE Orders ADD CancelRequestedFromStatus INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'CancelApproved')
    ALTER TABLE Orders ADD CancelApproved BIT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'CancelAdminNote')
    ALTER TABLE Orders ADD CancelAdminNote NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'CancelReviewedAt')
    ALTER TABLE Orders ADD CancelReviewedAt DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnReason')
    ALTER TABLE Orders ADD ReturnReason NVARCHAR(1000) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnImageUrl')
    ALTER TABLE Orders ADD ReturnImageUrl NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnRequestedAt')
    ALTER TABLE Orders ADD ReturnRequestedAt DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnRequestedFromStatus')
    ALTER TABLE Orders ADD ReturnRequestedFromStatus INT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnApproved')
    ALTER TABLE Orders ADD ReturnApproved BIT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnAdminNote')
    ALTER TABLE Orders ADD ReturnAdminNote NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Orders') AND name = 'ReturnReviewedAt')
    ALTER TABLE Orders ADD ReturnReviewedAt DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Reviews') AND name = 'IsApproved')
    ALTER TABLE Reviews ADD IsApproved BIT NOT NULL DEFAULT 0;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Reviews') AND name = 'UpdatedAt')
    ALTER TABLE Reviews ADD UpdatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Account') AND name = 'UpdatedAt')
    ALTER TABLE Account ADD UpdatedAt DATETIME DEFAULT GETDATE();
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Account') AND name = 'IsEmailVerified')
    ALTER TABLE Account ADD IsEmailVerified BIT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Account') AND name = 'AvatarUrl')
    ALTER TABLE Account ADD AvatarUrl NVARCHAR(500) NULL;
GO

UPDATE Account SET IsEmailVerified = 0 WHERE IsEmailVerified IS NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Account') AND name = 'EmailVerificationToken')
    ALTER TABLE Account ADD EmailVerificationToken NVARCHAR(100) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('Account') AND name = 'EmailVerificationTokenExpiresAt')
    ALTER TABLE Account ADD EmailVerificationTokenExpiresAt DATETIME NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE __EFMigrationsHistory (
        MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
        ProductVersion NVARCHAR(32) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260510125916_InitialCreate')
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260510125916_InitialCreate', '8.0.25');
GO

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260510140146_AddCartItemPriceColumns')
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260510140146_AddCartItemPriceColumns', '8.0.25');
GO

IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260515123000_AddOrderRequestFlow')
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260515123000_AddOrderRequestFlow', '8.0.25');
GO

-- =========================
-- CREATE WebSettings TABLE
-- =========================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WebSettings')
BEGIN
    CREATE TABLE WebSettings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SettingKey NVARCHAR(100) NOT NULL UNIQUE,
        SettingValue NVARCHAR(MAX) NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT NULL
    );
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE Object_ID = Object_ID('WebSettings') AND name = 'SettingValue')
    ALTER TABLE WebSettings ALTER COLUMN SettingValue NVARCHAR(MAX) NULL;
GO

-- Insert default web settings
IF NOT EXISTS (SELECT * FROM WebSettings WHERE SettingKey = 'SiteTitle')
BEGIN
    INSERT INTO WebSettings (SettingKey, SettingValue, CreatedAt) VALUES
    ('SiteTitle', 'Furnish - Premium Furniture Store', GETDATE()),
    ('SiteDescription', 'Discover premium quality furniture for every room in your home. Shop sofas, beds, dining sets, and more.', GETDATE()),
    ('LogoUrl', '', GETDATE()),
    ('ContactEmail', 'contact@furnish.com', GETDATE()),
    ('ContactPhone', '0901234567', GETDATE()),
    ('Address', '123 Furniture Street, District 1, Ho Chi Minh City', GETDATE()),
    ('FacebookUrl', 'https://facebook.com/furnish', GETDATE()),
    ('InstagramUrl', 'https://instagram.com/furnish', GETDATE()),
    ('FooterCopyright', '(c) 2024 Furnish. All rights reserved.', GETDATE());
END
GO

DECLARE @DefaultSettings TABLE (
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NULL
);

INSERT INTO @DefaultSettings (SettingKey, SettingValue) VALUES
('StoreName', 'Furnish'),
('SiteTagline', 'Premium Furniture'),
('AdminConsoleTitle', 'Furnish Admin'),
('AdminDashboardTitle', 'Dashboard'),
('FaviconUrl', '/favicon.ico'),
('PrimaryColor', '#d47f31'),
('AccentColor', '#1f2933'),
('DefaultLanguage', 'vi'),
('SeoKeywords', 'furniture, sofa, chair, table, home decor, noi that'),
('OpenGraphImageUrl', '/assets/images/slider/slider-img-1.png'),
('HeroTitle', '20% OFF'),
('HeroSubtitle', 'Comfy Sofa Home-Office'),
('HeroDescription', 'Premium comfort for your modern workspace'),
('HeroCtaText', 'Shop Now'),
('Feature1Title', 'Free Shipping'),
('Feature1Desc', 'On orders over 500,000 VND'),
('Feature2Title', 'Secure Payment'),
('Feature2Desc', '100% secure checkout'),
('Feature3Title', 'Easy Returns'),
('Feature3Desc', '30-day return policy'),
('Feature4Title', '24/7 Support'),
('Feature4Desc', 'Dedicated customer care'),
('NewsletterTitle', 'Subscribe to Our Newsletter'),
('NewsletterDesc', 'Get exclusive deals, new arrivals, and design inspiration delivered to your inbox.'),
('SupportEmail', 'support@furnish.com'),
('SalesEmail', 'sales@furnish.com'),
('Hotline', '1900 1234'),
('ZaloPhone', '0901234567'),
('WorkingHours', '08:00 - 21:00'),
('BusinessTaxCode', ''),
('FooterShortAbout', 'Premium furniture for modern homes.'),
('YouTubeUrl', ''),
('TikTokUrl', ''),
('MaintenanceMode', 'false'),
('MaintenanceTitle', 'Website under maintenance'),
('MaintenanceMessage', 'We are improving the shopping experience. Please come back soon.'),
('MaintenanceEstimatedBackAt', ''),
('MaintenanceSupportEmail', 'support@furnish.com'),
('EnableCOD', 'true'),
('EnableVNPay', 'true'),
('EnableMoMo', 'true'),
('EnableBankTransfer', 'false'),
('EnableReviews', 'true'),
('AutoApproveReviews', 'false'),
('EnableWishlist', 'true'),
('EnableNewsletterPopup', 'false'),
('EnableChatWidget', 'false'),
('ShowLowStockWarning', 'true'),
('RequireEmailVerification', 'false'),
('StandardShippingFee', '50000'),
('FreeShippingThreshold', '500000'),
('ReturnWindowDays', '30'),
('LowStockThreshold', '5'),
('ReturnPolicySummary', 'Products can be returned within 30 days if eligible and approved by our support team.'),
('GoogleAnalyticsId', ''),
('FacebookPixelId', ''),
('AnnouncementEnabled', 'false'),
('AnnouncementText', 'Free shipping for eligible orders.'),
('AnnouncementBgColor', '#d47f31'),
('AnnouncementLink', ''),
('PopupEnabled', 'false'),
('PopupTitle', 'Welcome!'),
('PopupContent', 'Get the latest offers and new arrivals from our store.'),
('PopupImageUrl', ''),
('PopupDelay', '3000'),
('CustomHeadCode', ''),
('CustomFooterCode', ''),
('PrivacyPolicyHtml', ''),
('TermsOfServiceHtml', ''),
('ShippingPolicyHtml', ''),
('VNPayUrl', 'https://sandbox.vnpayment.vn/paymentv2/vpcpay.html'),
('VNPayMerchantId', ''),
('VNPayMerchantSecret', ''),
('MoMoUrl', 'https://test-payment.momo.vn/v2/gateway/api/create'),
('MoMoPartnerCode', ''),
('MoMoAccessKey', ''),
('MoMoSecretKey', '');

INSERT INTO WebSettings (SettingKey, SettingValue, CreatedAt)
SELECT ds.SettingKey, ds.SettingValue, GETDATE()
FROM @DefaultSettings ds
WHERE NOT EXISTS (
    SELECT 1
    FROM WebSettings ws
    WHERE ws.SettingKey = ds.SettingKey
);
GO

-- =========================
-- FULL RESET if partial data exists
-- =========================
DECLARE @CategoryCount INT = (SELECT COUNT(*) FROM Categories);
DECLARE @SupplierCount INT = (SELECT COUNT(*) FROM Suppliers);

IF @CategoryCount > 0 OR @SupplierCount > 0
BEGIN
    -- Cascade delete all child data first, then parents
    DELETE FROM ProductImages;
    DELETE FROM ProductVariants;
    DELETE FROM OrderItems;
    DELETE FROM Orders;
    DELETE FROM Products;
    DELETE FROM Categories;
    DELETE FROM Suppliers;

    -- Reset all identity columns
    DBCC CHECKIDENT ('Categories', RESEED, 0);
    DBCC CHECKIDENT ('Suppliers', RESEED, 0);
    DBCC CHECKIDENT ('Products', RESEED, 0);
    DBCC CHECKIDENT ('ProductVariants', RESEED, 0);
    DBCC CHECKIDENT ('ProductImages', RESEED, 0);
END
GO

-- =========================
-- SUPPLIERS (only CompanyName and Phone)
-- =========================
INSERT INTO Suppliers (CompanyName, Phone, CreatedAt) VALUES
('Vietnam Furniture Co', '0901234567', GETDATE()),
('Asia Timber Works', '0902345678', GETDATE()),
('Modern Living Ltd', '0903456789', GETDATE()),
('EcoWood Production', '0904567890', GETDATE()),
('Golden Oak Furnish', '0905678901', GETDATE());
GO

-- =========================
-- CATEGORIES
-- =========================
INSERT INTO Categories (CategoryName, Description, CreatedAt) VALUES
('Living Room', 'Sofas, armchairs, and living room essentials', GETDATE()),
('Bedroom', 'Beds, mattresses, wardrobes, and bedroom furniture', GETDATE()),
('Dining Room', 'Dining tables, chairs, and dining sets', GETDATE()),
('Office', 'Desks, office chairs, and workspace furniture', GETDATE()),
('Decor', 'Wall art, vases, mirrors, and decorative pieces', GETDATE()),
('Outdoor', 'Patio furniture and outdoor living', GETDATE());
GO

-- =========================
-- PRODUCTS (100 items)
-- =========================
INSERT INTO Products (ProductName, CategoryID, SupplierID, QuantityPerUnit, UnitPrice, Discontinued, CreatedAt) VALUES
-- Living Room (1-20)
('Modern L-Shaped Sofa', 1, 1, '1 set', 15990000, 0, GETDATE()),
('Classic Leather Armchair', 1, 1, '1 piece', 4990000, 0, GETDATE()),
('Velvet Sectional Sofa', 1, 3, '1 set', 22990000, 0, GETDATE()),
('Nordic 3-Seater Sofa', 1, 1, '1 set', 8990000, 0, GETDATE()),
('Minimalist Coffee Table', 1, 2, '1 piece', 1990000, 0, GETDATE()),
('Oak Wood TV Stand', 1, 2, '1 piece', 3490000, 0, GETDATE()),
('Fabric Loveseat', 1, 3, '1 set', 6990000, 0, GETDATE()),
('Recliner Massage Chair', 1, 1, '1 piece', 12990000, 0, GETDATE()),
('Corner Floor Lamp', 1, 4, '1 piece', 890000, 0, GETDATE()),
('Wall Shelf Unit', 1, 2, '1 set', 1590000, 0, GETDATE()),
('Storage Ottoman', 1, 3, '1 piece', 1290000, 0, GETDATE()),
('Glass Top Side Table', 1, 4, '1 piece', 990000, 0, GETDATE()),
('Modular Bookshelf', 1, 2, '1 set', 4290000, 0, GETDATE()),
('Chaise Lounge Chair', 1, 1, '1 piece', 5990000, 0, GETDATE()),
('Slim Console Table', 1, 3, '1 piece', 1790000, 0, GETDATE()),
('Bean Bag Large', 1, 4, '1 piece', 1490000, 0, GETDATE()),
('Metal Floor Lamp', 1, 2, '1 piece', 690000, 0, GETDATE()),
('Curved Accent Chair', 1, 1, '1 piece', 3890000, 0, GETDATE()),
('Marble Top Table', 1, 3, '1 piece', 4590000, 0, GETDATE()),
('Rattan Peacock Chair', 1, 4, '1 piece', 2790000, 0, GETDATE()),

-- Bedroom (21-40)
('King Size Platform Bed', 2, 1, '1 set', 18990000, 0, GETDATE()),
('Upholstered Queen Bed', 2, 3, '1 set', 12990000, 0, GETDATE()),
('Wooden Bunk Bed', 2, 2, '1 set', 8990000, 0, GETDATE()),
('Memory Foam Mattress', 2, 1, '1 piece', 5990000, 0, GETDATE()),
('Latex Mattress Queen', 2, 4, '1 piece', 7990000, 0, GETDATE()),
('Sliding Door Wardrobe', 2, 2, '1 piece', 14990000, 0, GETDATE()),
('2-Door Wardrobe', 2, 3, '1 piece', 6990000, 0, GETDATE()),
('Nightstand Set (2)', 2, 1, '1 set', 1890000, 0, GETDATE()),
('Dresser with Mirror', 2, 2, '1 piece', 4990000, 0, GETDATE()),
('King Mattress Topper', 2, 4, '1 piece', 990000, 0, GETDATE()),
('Pillow Set (4pcs)', 2, 1, '1 set', 590000, 0, GETDATE()),
('Bedding Set King', 2, 3, '1 set', 1990000, 0, GETDATE()),
('Underbed Storage', 2, 2, '1 piece', 790000, 0, GETDATE()),
('Bedside Cabinet', 2, 1, '1 piece', 1290000, 0, GETDATE()),
('Walk-in Closet System', 2, 2, '1 set', 24990000, 0, GETDATE()),
('Kids Bed Frame', 2, 4, '1 piece', 3990000, 0, GETDATE()),
('Folding Mattress', 2, 1, '1 piece', 1490000, 0, GETDATE()),
('Blanket Comforter Set', 2, 3, '1 set', 1290000, 0, GETDATE()),
('Vanity Table with Stool', 2, 2, '1 set', 3490000, 0, GETDATE()),
('Platform Storage Bed', 2, 1, '1 piece', 8990000, 0, GETDATE()),

-- Dining Room (41-60)
('Oak Extendable Dining Table', 3, 2, '1 piece', 8990000, 0, GETDATE()),
('Glass Top Dining Set (6)', 3, 3, '1 set', 15990000, 0, GETDATE()),
('Rattan Dining Chairs (4)', 3, 4, '1 set', 4990000, 0, GETDATE()),
('Marble Dining Table', 3, 1, '1 piece', 18990000, 0, GETDATE()),
('Wooden Dining Chairs (4)', 3, 2, '1 set', 3990000, 0, GETDATE()),
('Farmhouse Table 8-seater', 3, 1, '1 piece', 12990000, 0, GETDATE()),
('Modern Dining Chairs (4)', 3, 3, '1 set', 2990000, 0, GETDATE()),
('Drop-leaf Table', 3, 2, '1 piece', 3490000, 0, GETDATE()),
('Bar Height Table Set', 3, 4, '1 set', 5990000, 0, GETDATE()),
('Retro Dining Set', 3, 1, '1 set', 7990000, 0, GETDATE()),
('Ceramic Dining Set (6)', 3, 3, '1 set', 11990000, 0, GETDATE()),
('Stackable Dining Chairs', 3, 2, '1 set', 1990000, 0, GETDATE()),
('Expandable Dining Table', 3, 4, '1 piece', 6990000, 0, GETDATE()),
('Round Dining Table 4-seater', 3, 1, '1 piece', 4490000, 0, GETDATE()),
('Leather Dining Chairs (6)', 3, 3, '1 set', 8990000, 0, GETDATE()),
('Industrial Dining Set', 3, 2, '1 set', 7490000, 0, GETDATE()),
('Bamboo Dining Table', 3, 4, '1 piece', 3990000, 0, GETDATE()),
('Velvet Dining Chairs (4)', 3, 1, '1 set', 4990000, 0, GETDATE()),
('Concrete Dining Table', 3, 3, '1 piece', 15990000, 0, GETDATE()),
('Compact Dining Set 4-seater', 3, 2, '1 set', 3290000, 0, GETDATE()),

-- Office (61-80)
('L-Shaped Executive Desk', 4, 2, '1 piece', 8990000, 0, GETDATE()),
('Ergonomic Office Chair', 4, 1, '1 piece', 2990000, 0, GETDATE()),
('Standing Desk Adjustable', 4, 3, '1 piece', 6990000, 0, GETDATE()),
('Executive Leather Chair', 4, 1, '1 piece', 4490000, 0, GETDATE()),
('Computer Desk Wood', 4, 2, '1 piece', 1990000, 0, GETDATE()),
('Office Bookshelf', 4, 3, '1 piece', 2990000, 0, GETDATE()),
('Filing Cabinet 4-drawer', 4, 2, '1 piece', 1890000, 0, GETDATE()),
('Monitor Stand Riser', 4, 1, '1 piece', 390000, 0, GETDATE()),
('Desk Organizer Set', 4, 4, '1 set', 290000, 0, GETDATE()),
('Ergonomic Footrest', 4, 3, '1 piece', 490000, 0, GETDATE()),
('Mesh Office Chair', 4, 1, '1 piece', 1990000, 0, GETDATE()),
('Corner Office Desk', 4, 2, '1 piece', 5490000, 0, GETDATE()),
('Mobile Pedestal', 4, 3, '1 piece', 990000, 0, GETDATE()),
('Desk Lamp LED', 4, 4, '1 piece', 590000, 0, GETDATE()),
('Whiteboard 90x120cm', 4, 2, '1 piece', 690000, 0, GETDATE()),
('Office Partitions (3)', 4, 3, '1 set', 2990000, 0, GETDATE()),
('Leather Executive Set', 4, 1, '1 set', 12990000, 0, GETDATE()),
('Compact Study Desk', 4, 2, '1 piece', 1490000, 0, GETDATE()),
('Gaming Chair Pro', 4, 1, '1 piece', 3990000, 0, GETDATE()),
('Workstation Bundle', 4, 3, '1 set', 8990000, 0, GETDATE()),

-- Decor (81-90)
('Abstract Canvas Art', 5, 4, '1 piece', 990000, 0, GETDATE()),
('Ceramic Vase Large', 5, 4, '1 piece', 690000, 0, GETDATE()),
('Decorative Mirror Round', 5, 4, '1 piece', 1490000, 0, GETDATE()),
('Artificial Plant Tree', 5, 4, '1 piece', 1290000, 0, GETDATE()),
('Wall Clock Modern', 5, 4, '1 piece', 490000, 0, GETDATE()),
('Sculpture Statue', 5, 4, '1 piece', 1990000, 0, GETDATE()),
('LED Wall Sconce', 5, 4, '1 piece', 590000, 0, GETDATE()),
('Picture Frame Set', 5, 4, '1 set', 390000, 0, GETDATE()),
('Candle Holder Set', 5, 4, '1 set', 290000, 0, GETDATE()),
('Tapestry Wall Hanging', 5, 4, '1 piece', 690000, 0, GETDATE()),

-- Outdoor (91-100)
('Rattan Garden Set (4)', 6, 4, '1 set', 8990000, 0, GETDATE()),
('Teak Outdoor Chair', 6, 2, '1 piece', 1990000, 0, GETDATE()),
('Patio Dining Set (6)', 6, 4, '1 set', 15990000, 0, GETDATE()),
('Lounger Pool Chair', 6, 4, '1 piece', 2490000, 0, GETDATE()),
('Garden Umbrella', 6, 4, '1 piece', 1490000, 0, GETDATE()),
('Outdoor Coffee Table', 6, 2, '1 piece', 1990000, 0, GETDATE()),
('Hammock Swing Chair', 6, 4, '1 piece', 2990000, 0, GETDATE()),
('Steel Garden Bench', 6, 2, '1 piece', 3490000, 0, GETDATE()),
('Outdoor Sectional Set', 6, 4, '1 set', 19990000, 0, GETDATE()),
('Folding Garden Stool', 6, 4, '1 piece', 390000, 0, GETDATE());
GO

-- =========================
-- PRODUCT VARIANTS (Multiple per product - Colors & Sizes)
-- =========================
-- Living Room Products (1-20): Sofas, Chairs with colors and sizes
INSERT INTO ProductVariants (ProductId, SKU, Color, Size, PriceAdjustment, StockQuantity, IsActive, CreatedAt) VALUES
(1, 'LSS-GRY-L', 'Gray', 'Large', 0, 15, 1, GETDATE()),
(1, 'LSS-GRY-M', 'Gray', 'Medium', -500000, 10, 1, GETDATE()),
(1, 'LSS-BLU-L', 'Navy Blue', 'Large', 0, 12, 1, GETDATE()),
(1, 'LSS-CRM-L', 'Cream', 'Large', 300000, 8, 1, GETDATE()),
(2, 'CLA-BLK', 'Black', 'One Size', 0, 20, 1, GETDATE()),
(2, 'CLA-BRW', 'Brown', 'One Size', 0, 15, 1, GETDATE()),
(2, 'CLA-TAN', 'Tan', 'One Size', 200000, 10, 1, GETDATE()),
(3, 'VSO-NVY-L', 'Navy', 'Large', 0, 8, 1, GETDATE()),
(3, 'VSO-EMR-L', 'Emerald', 'Large', 500000, 6, 1, GETDATE()),
(3, 'VSO-BRG-L', 'Burgundy', 'Large', 0, 7, 1, GETDATE()),
(4, 'N3S-WHT-L', 'White', 'Large', 0, 18, 1, GETDATE()),
(4, 'N3S-WHT-M', 'White', 'Medium', -300000, 12, 1, GETDATE()),
(4, 'N3S-GRY-L', 'Gray', 'Large', 0, 10, 1, GETDATE()),
(5, 'MCT-OAK', 'Oak', 'One Size', 0, 25, 1, GETDATE()),
(5, 'MCT-WAL', 'Walnut', 'One Size', 400000, 15, 1, GETDATE()),
(6, 'OTV-OAK-120', 'Oak', '120cm', 0, 12, 1, GETDATE()),
(6, 'OTV-OAK-150', 'Oak', '150cm', 300000, 8, 1, GETDATE()),
(6, 'OTV-BLK-120', 'Black', '120cm', 0, 10, 1, GETDATE()),
(7, 'FLV-GRN', 'Green', 'One Size', 0, 14, 1, GETDATE()),
(7, 'FLV-BLU', 'Blue', 'One Size', 0, 10, 1, GETDATE()),
(7, 'FLV-RED', 'Red', 'One Size', 200000, 8, 1, GETDATE()),
(8, 'RMC-BLK', 'Black', 'One Size', 0, 6, 1, GETDATE()),
(8, 'RMC-BRN', 'Brown', 'One Size', 0, 8, 1, GETDATE()),
(9, 'CFL-BLK', 'Black', 'One Size', 0, 30, 1, GETDATE()),
(9, 'CFL-CHR', 'Chrome', 'One Size', 150000, 20, 1, GETDATE()),
(10, 'WSU-WHT', 'White', 'One Size', 0, 15, 1, GETDATE()),
(10, 'WSU-OAK', 'Oak', 'One Size', 200000, 12, 1, GETDATE()),
(11, 'SOT-GRY', 'Gray', 'One Size', 0, 20, 1, GETDATE()),
(11, 'SOT-BLU', 'Blue', 'One Size', 100000, 15, 1, GETDATE()),
(12, 'GST-SLV', 'Silver', 'One Size', 0, 18, 1, GETDATE()),
(12, 'GST-GLD', 'Gold', 'One Size', 200000, 10, 1, GETDATE()),
(13, 'MSB-WHT', 'White', 'One Size', 0, 8, 1, GETDATE()),
(13, 'MSB-BLK', 'Black', 'One Size', 150000, 6, 1, GETDATE()),
(14, 'CLN-CRM', 'Cream', 'One Size', 0, 10, 1, GETDATE()),
(14, 'CLN-GRY', 'Gray', 'One Size', 0, 8, 1, GETDATE()),
(15, 'SCT-WHT', 'White', 'One Size', 0, 22, 1, GETDATE()),
(15, 'SCT-BLK', 'Black', 'One Size', 100000, 15, 1, GETDATE()),
(16, 'BBG-GRN', 'Green', 'Large', 0, 12, 1, GETDATE()),
(16, 'BBG-GRY', 'Gray', 'Large', 0, 10, 1, GETDATE()),
(17, 'MFL-BLK', 'Black', 'One Size', 0, 25, 1, GETDATE()),
(17, 'MFL-BRS', 'Brass', 'One Size', 300000, 18, 1, GETDATE()),
(18, 'CAC-TUR', 'Turquoise', 'One Size', 0, 8, 1, GETDATE()),
(18, 'CAC-MST', 'Mustard', 'One Size', 0, 10, 1, GETDATE()),
(18, 'CAC-DKV', 'Dark Velvet', 'One Size', 200000, 7, 1, GETDATE()),
(19, 'MTT-WHT', 'White', 'One Size', 0, 14, 1, GETDATE()),
(19, 'MTT-BLK', 'Black', 'One Size', 250000, 10, 1, GETDATE()),
(20, 'RPC-YLW', 'Yellow', 'One Size', 0, 8, 1, GETDATE()),
(20, 'RPC-NAT', 'Natural', 'One Size', 0, 12, 1, GETDATE());

-- Bedroom Products (21-40): Beds, Mattresses, Wardrobes with sizes
INSERT INTO ProductVariants (ProductId, SKU, Color, Size, PriceAdjustment, StockQuantity, IsActive, CreatedAt) VALUES
(21, 'KPB-WHT-K', 'White', 'King', 0, 5, 1, GETDATE()),
(21, 'KPB-WHT-Q', 'White', 'Queen', -1500000, 6, 1, GETDATE()),
(21, 'KPB-OAK-K', 'Oak', 'King', 1000000, 4, 1, GETDATE()),
(22, 'UPB-BLU-Q', 'Blue', 'Queen', 0, 8, 1, GETDATE()),
(22, 'UPB-GRY-Q', 'Gray', 'Queen', 0, 10, 1, GETDATE()),
(22, 'UPB-BLU-K', 'Blue', 'King', 500000, 6, 1, GETDATE()),
(23, 'WBB-WHT-2P', 'White', '2 Person', 0, 4, 1, GETDATE()),
(23, 'WBB-OAK-2P', 'Oak', '2 Person', 800000, 3, 1, GETDATE()),
(24, 'MFM-STD', 'Standard', 'Queen', 0, 15, 1, GETDATE()),
(24, 'MFM-FRM', 'Firm', 'Queen', 500000, 10, 1, GETDATE()),
(24, 'MFM-STD-K', 'Standard', 'King', 1000000, 8, 1, GETDATE()),
(25, 'LTM-GLD-Q', 'Golden', 'Queen', 0, 6, 1, GETDATE()),
(25, 'LTM-GLD-K', 'Golden', 'King', 800000, 4, 1, GETDATE()),
(26, 'SDW-WHT-180', 'White', '180cm', 0, 3, 1, GETDATE()),
(26, 'SDW-OAK-180', 'Oak', '180cm', 1500000, 2, 1, GETDATE()),
(26, 'SDW-BLK-180', 'Black', '180cm', 0, 4, 1, GETDATE()),
(27, '2DW-WHT', 'White', 'One Size', 0, 8, 1, GETDATE()),
(27, '2DW-OAK', 'Oak', 'One Size', 600000, 5, 1, GETDATE()),
(28, 'NSS-WHT', 'White', 'One Size', 0, 20, 1, GETDATE()),
(28, 'NSS-BLK', 'Black', 'One Size', 200000, 15, 1, GETDATE()),
(29, 'DRS-OAK', 'Oak', 'One Size', 0, 6, 1, GETDATE()),
(29, 'DRS-WHT', 'White', 'One Size', 300000, 8, 1, GETDATE()),
(30, 'KMT-WHT', 'White', 'King', 0, 15, 1, GETDATE()),
(31, 'PLS-WHT', 'White', 'Standard', 0, 50, 1, GETDATE()),
(31, 'PLS-CRM', 'Cream', 'Standard', 0, 40, 1, GETDATE()),
(32, 'BDS-GRY-K', 'Gray', 'King', 0, 10, 1, GETDATE()),
(32, 'BDS-WHT-K', 'White', 'King', 200000, 8, 1, GETDATE()),
(33, 'UBS-WHT', 'White', 'One Size', 0, 25, 1, GETDATE()),
(34, 'BSC-WHT', 'White', 'One Size', 0, 18, 1, GETDATE()),
(34, 'BSC-BLK', 'Black', 'One Size', 150000, 12, 1, GETDATE()),
(35, 'WCS-WHT', 'White', 'One Size', 0, 3, 1, GETDATE()),
(36, 'KBF-BLU', 'Blue', 'Single', 0, 8, 1, GETDATE()),
(36, 'KBF-PNK', 'Pink', 'Single', 0, 6, 1, GETDATE()),
(37, 'FLM-WHT', 'White', 'Single', 0, 15, 1, GETDATE()),
(37, 'FLM-GRY', 'Gray', 'Single', 100000, 10, 1, GETDATE()),
(38, 'BCS-NVY', 'Navy', 'Queen', 0, 12, 1, GETDATE()),
(38, 'BCS-CRM', 'Cream', 'Queen', 0, 10, 1, GETDATE()),
(39, 'VNT-WHT', 'White', 'One Size', 0, 6, 1, GETDATE()),
(39, 'VNT-PNK', 'Pink', 'One Size', 200000, 4, 1, GETDATE()),
(40, 'PSB-OAK-Q', 'Oak', 'Queen', 0, 5, 1, GETDATE()),
(40, 'PSB-OAK-K', 'Oak', 'King', 1000000, 3, 1, GETDATE());

-- Dining Room Products (41-60): Tables and Chairs with finishes and sizes
INSERT INTO ProductVariants (ProductId, SKU, Color, Size, PriceAdjustment, StockQuantity, IsActive, CreatedAt) VALUES
(41, 'EDT-OAK-6S', 'Oak', '6-Seater', 0, 6, 1, GETDATE()),
(41, 'EDT-OAK-4S', 'Oak', '4-Seater', -1500000, 8, 1, GETDATE()),
(41, 'EDT-BLK-6S', 'Black', '6-Seater', 500000, 4, 1, GETDATE()),
(42, 'GDT-SLV-6', 'Silver', '6-Seater', 0, 4, 1, GETDATE()),
(42, 'GDT-GLD-6', 'Gold', '6-Seater', 1000000, 3, 1, GETDATE()),
(43, 'RDC-NAT-4', 'Natural', '4-Piece', 0, 10, 1, GETDATE()),
(43, 'RDC-BLK-4', 'Black', '4-Piece', 200000, 8, 1, GETDATE()),
(44, 'MDT-WHT-8', 'White Marble', '8-Seater', 0, 3, 1, GETDATE()),
(44, 'MDT-BLK-8', 'Black Marble', '8-Seater', 2000000, 2, 1, GETDATE()),
(45, 'WDC-OAK-4', 'Oak', '4-Piece', 0, 15, 1, GETDATE()),
(45, 'WDC-WHT-4', 'White', '4-Piece', 0, 12, 1, GETDATE()),
(46, 'FHT-OAK-8', 'Oak', '8-Seater', 0, 4, 1, GETDATE()),
(46, 'FHT-WHT-8', 'White', '8-Seater', 800000, 3, 1, GETDATE()),
(47, 'MDC-BLK-4', 'Black', '4-Piece', 0, 12, 1, GETDATE()),
(47, 'MDC-WHT-4', 'White', '4-Piece', 0, 10, 1, GETDATE()),
(48, 'DLT-OAK-4', 'Oak', '4-Seater', 0, 8, 1, GETDATE()),
(48, 'DLT-BLK-4', 'Black', '4-Seater', 300000, 6, 1, GETDATE()),
(49, 'BHT-WHT', 'White', '3-Piece', 0, 6, 1, GETDATE()),
(49, 'BHT-BLK', 'Black', '3-Piece', 500000, 4, 1, GETDATE()),
(50, 'RDS-OAK-6', 'Oak', '6-Piece', 0, 5, 1, GETDATE()),
(50, 'RDS-CRM-6', 'Cream', '6-Piece', 400000, 4, 1, GETDATE()),
(51, 'CDS-CRM-6', 'Cream', '6-Piece', 0, 4, 1, GETDATE()),
(51, 'CDS-WHT-6', 'White', '6-Piece', 0, 5, 1, GETDATE()),
(52, 'SDC-BLK-4', 'Black', '4-Piece', 0, 20, 1, GETDATE()),
(52, 'SDC-WHT-4', 'White', '4-Piece', 0, 18, 1, GETDATE()),
(53, 'EDT2-OAK-6', 'Oak', '6-Seater', 0, 7, 1, GETDATE()),
(53, 'EDT2-WHT-6', 'White', '6-Seater', 300000, 5, 1, GETDATE()),
(54, 'RDT-WHT-4', 'White', '4-Seater', 0, 10, 1, GETDATE()),
(54, 'RDT-BLK-4', 'Black', '4-Seater', 200000, 8, 1, GETDATE()),
(55, 'LDC-BLK-6', 'Black', '6-Piece', 0, 6, 1, GETDATE()),
(55, 'LDC-BRW-6', 'Brown', '6-Piece', 0, 5, 1, GETDATE()),
(56, 'IDS-OAK-6', 'Oak', '6-Piece', 0, 5, 1, GETDATE()),
(56, 'IDS-BLK-6', 'Black', '6-Piece', 400000, 4, 1, GETDATE()),
(57, 'BDT-NAT', 'Natural', '6-Seater', 0, 8, 1, GETDATE()),
(57, 'BDT-BLK', 'Black', '6-Seater', 300000, 6, 1, GETDATE()),
(58, 'VDC-GRN-4', 'Green', '4-Piece', 0, 10, 1, GETDATE()),
(58, 'VDC-BLU-4', 'Blue', '4-Piece', 0, 8, 1, GETDATE()),
(59, 'CDT-GRY', 'Gray', '6-Seater', 0, 4, 1, GETDATE()),
(59, 'CDT-WHT', 'White', '6-Seater', 800000, 3, 1, GETDATE()),
(60, 'CDS-OAK-4', 'Oak', '4-Piece', 0, 12, 1, GETDATE()),
(60, 'CDS-WHT-4', 'White', '4-Piece', 0, 10, 1, GETDATE());

-- Office Products (61-80): Desks, Chairs with colors and sizes
INSERT INTO ProductVariants (ProductId, SKU, Color, Size, PriceAdjustment, StockQuantity, IsActive, CreatedAt) VALUES
(61, 'LED-WHT-L', 'White', 'Large', 0, 4, 1, GETDATE()),
(61, 'LED-BLK-L', 'Black', 'Large', 500000, 3, 1, GETDATE()),
(61, 'LED-OAK-L', 'Oak', 'Large', 800000, 2, 1, GETDATE()),
(62, 'EOC-BLK-M', 'Black', 'Medium', 0, 15, 1, GETDATE()),
(62, 'EOC-GRY-M', 'Gray', 'Medium', 0, 12, 1, GETDATE()),
(62, 'EOC-NVY-M', 'Navy', 'Medium', 200000, 10, 1, GETDATE()),
(63, 'STD-WHT-120', 'White', '120cm', 0, 6, 1, GETDATE()),
(63, 'STD-BLK-120', 'Black', '120cm', 300000, 5, 1, GETDATE()),
(63, 'STD-WHT-140', 'White', '140cm', 500000, 4, 1, GETDATE()),
(64, 'EXC-BLK', 'Black', 'One Size', 0, 8, 1, GETDATE()),
(64, 'EXC-BRW', 'Brown', 'One Size', 400000, 6, 1, GETDATE()),
(65, 'CDK-OAK-120', 'Oak', '120cm', 0, 12, 1, GETDATE()),
(65, 'CDK-WHT-120', 'White', '120cm', 0, 10, 1, GETDATE()),
(66, 'OBS-WHT', 'White', 'One Size', 0, 8, 1, GETDATE()),
(66, 'OBS-BLK', 'Black', 'One Size', 200000, 6, 1, GETDATE()),
(67, 'FLC-BLK-4D', 'Black', '4-Drawer', 0, 10, 1, GETDATE()),
(67, 'FLC-GRY-4D', 'Gray', '4-Drawer', 0, 8, 1, GETDATE()),
(68, 'MST-BLK', 'Black', 'One Size', 0, 30, 1, GETDATE()),
(68, 'MST-WHT', 'White', 'One Size', 100000, 25, 1, GETDATE()),
(69, 'DOS-WHT', 'White', 'One Size', 0, 20, 1, GETDATE()),
(69, 'DOS-BLK', 'Black', 'One Size', 0, 18, 1, GETDATE()),
(70, 'EFR-BLK', 'Black', 'One Size', 0, 15, 1, GETDATE()),
(71, 'MOC-BLK', 'Black', 'One Size', 0, 18, 1, GETDATE()),
(71, 'MOC-GRY', 'Gray', 'One Size', 0, 15, 1, GETDATE()),
(71, 'MOC-NVY', 'Navy', 'One Size', 150000, 12, 1, GETDATE()),
(72, 'COD-WHT', 'White', 'One Size', 0, 8, 1, GETDATE()),
(72, 'COD-OAK', 'Oak', 'One Size', 400000, 6, 1, GETDATE()),
(73, 'MPD-WHT', 'White', 'One Size', 0, 20, 1, GETDATE()),
(73, 'MPD-BLK', 'Black', 'One Size', 0, 18, 1, GETDATE()),
(74, 'DLM-WHT', 'White', 'One Size', 0, 25, 1, GETDATE()),
(74, 'DLM-BLK', 'Black', 'One Size', 150000, 20, 1, GETDATE()),
(75, 'WBD-WHT', 'White', '90x120cm', 0, 12, 1, GETDATE()),
(75, 'WBD-BLK', 'Black', '90x120cm', 100000, 10, 1, GETDATE()),
(76, 'OFP-GRY-3', 'Gray', '3-Piece', 0, 6, 1, GETDATE()),
(76, 'OFP-BLK-3', 'Black', '3-Piece', 300000, 4, 1, GETDATE()),
(77, 'LES-BLK', 'Black', 'One Size', 0, 3, 1, GETDATE()),
(77, 'LES-BRW', 'Brown', 'One Size', 500000, 2, 1, GETDATE()),
(78, 'CSD-WHT', 'White', 'One Size', 0, 15, 1, GETDATE()),
(78, 'CSD-OAK', 'Oak', 'One Size', 200000, 12, 1, GETDATE()),
(79, 'GCP-BLK-R', 'Black', 'Racing', 0, 10, 1, GETDATE()),
(79, 'GCP-RED-R', 'Red', 'Racing', 300000, 8, 1, GETDATE()),
(79, 'GCP-WHT-R', 'White', 'Racing', 0, 6, 1, GETDATE()),
(80, 'WKB-OAK', 'Oak', 'One Size', 0, 4, 1, GETDATE()),
(80, 'WKB-WHT', 'White', 'One Size', 800000, 3, 1, GETDATE());

-- Decor Products (81-90): Various styles
INSERT INTO ProductVariants (ProductId, SKU, Color, Size, PriceAdjustment, StockQuantity, IsActive, CreatedAt) VALUES
(81, 'ACA-ABM', 'Abstract Mix', 'Large', 0, 20, 1, GETDATE()),
(81, 'ACA-BLM', 'Black & White', 'Large', 200000, 15, 1, GETDATE()),
(81, 'ACA-NAT', 'Nature', 'Medium', -100000, 18, 1, GETDATE()),
(82, 'CVL-WHT-L', 'White', 'Large', 0, 25, 1, GETDATE()),
(82, 'CVL-BLU-L', 'Blue', 'Large', 100000, 20, 1, GETDATE()),
(82, 'CVL-GRY-L', 'Gray', 'Large', 0, 22, 1, GETDATE()),
(83, 'DMR-GLD-R', 'Gold', 'Round', 0, 15, 1, GETDATE()),
(83, 'DMR-SLV-R', 'Silver', 'Round', -100000, 18, 1, GETDATE()),
(83, 'DMR-BLK-R', 'Black', 'Round', 0, 12, 1, GETDATE()),
(84, 'APT-GRN-L', 'Green', 'Large', 0, 20, 1, GETDATE()),
(84, 'APT-GRN-M', 'Green', 'Medium', -200000, 25, 1, GETDATE()),
(85, 'WCM-BLK', 'Black', 'One Size', 0, 30, 1, GETDATE()),
(85, 'WCM-GLD', 'Gold', 'One Size', 100000, 25, 1, GETDATE()),
(85, 'WCM-WHT', 'White', 'One Size', 0, 28, 1, GETDATE()),
(86, 'SST-BRS', 'Bronze', 'Medium', 0, 12, 1, GETDATE()),
(86, 'SST-BLK', 'Black', 'Medium', 0, 10, 1, GETDATE()),
(87, 'LWS-WHT', 'White', 'One Size', 0, 25, 1, GETDATE()),
(87, 'LWS-BLK', 'Black', 'One Size', 80000, 20, 1, GETDATE()),
(88, 'PFS-WHT-5P', 'White', '5-Piece', 0, 30, 1, GETDATE()),
(88, 'PFS-BLK-5P', 'Black', '5-Piece', 100000, 25, 1, GETDATE()),
(89, 'CHD-GLD-3P', 'Gold', '3-Piece', 0, 20, 1, GETDATE()),
(89, 'CHD-SLV-3P', 'Silver', '3-Piece', -50000, 25, 1, GETDATE()),
(90, 'TWH-NVY', 'Navy', 'Large', 0, 18, 1, GETDATE()),
(90, 'TWH-GRY', 'Gray', 'Large', 0, 15, 1, GETDATE()),
(90, 'TWH-EMD', 'Emerald', 'Large', 100000, 12, 1, GETDATE());

-- Outdoor Products (91-100): Weather-resistant colors
INSERT INTO ProductVariants (ProductId, SKU, Color, Size, PriceAdjustment, StockQuantity, IsActive, CreatedAt) VALUES
(91, 'RGS-GRY-4', 'Gray', '4-Piece', 0, 5, 1, GETDATE()),
(91, 'RGS-BLK-4', 'Black', '4-Piece', 500000, 4, 1, GETDATE()),
(91, 'RGS-BRN-4', 'Brown', '4-Piece', 0, 6, 1, GETDATE()),
(92, 'TOC-TEK', 'Teak', 'One Size', 0, 12, 1, GETDATE()),
(92, 'TOC-GRY', 'Gray', 'One Size', -200000, 15, 1, GETDATE()),
(93, 'PDS-WHT-6', 'White', '6-Piece', 0, 3, 1, GETDATE()),
(93, 'PDS-TFK-6', 'Teak', '6-Piece', 1000000, 2, 1, GETDATE()),
(94, 'LPC-GRY', 'Gray', 'One Size', 0, 10, 1, GETDATE()),
(94, 'LPC-WHT', 'White', 'One Size', 200000, 8, 1, GETDATE()),
(95, 'GUM-CRM', 'Cream', 'One Size', 0, 8, 1, GETDATE()),
(95, 'GUM-BLK', 'Black', 'One Size', 300000, 6, 1, GETDATE()),
(96, 'OCT-TEK', 'Teak', 'One Size', 0, 10, 1, GETDATE()),
(96, 'OCT-GRY', 'Gray', 'One Size', -300000, 12, 1, GETDATE()),
(97, 'HSC-NAT', 'Natural', 'One Size', 0, 8, 1, GETDATE()),
(97, 'HSC-BLK', 'Black', 'One Size', 400000, 6, 1, GETDATE()),
(98, 'SGB-BLK', 'Black', 'One Size', 0, 6, 1, GETDATE()),
(98, 'SGB-GRN', 'Green', 'One Size', 200000, 5, 1, GETDATE()),
(99, 'OSS-GRY-L', 'Gray', 'Large', 0, 2, 1, GETDATE()),
(99, 'OSS-BLK-L', 'Black', 'Large', 800000, 2, 1, GETDATE()),
(99, 'OSS-BRN-L', 'Brown', 'Large', 0, 3, 1, GETDATE()),
(100, 'FGS-WHT', 'White', 'One Size', 0, 20, 1, GETDATE()),
(100, 'FGS-BLK', 'Black', 'One Size', 50000, 15, 1, GETDATE());

-- =========================
-- PRODUCT IMAGES (Multiple per product)
-- =========================
-- Main images for all products (cycling through 8 images)
INSERT INTO ProductImages (ProductId, ImageUrl, IsMain, DisplayOrder, CreatedAt)
SELECT Id,
    '/assets/images/product-img-' + CAST((ROW_NUMBER() OVER (ORDER BY Id) - 1) % 8 + 1 AS VARCHAR(10)) + '.jpg',
    1, 0, GETDATE()
FROM Products;
GO

-- Additional images for product variants (alternate angles/colors)
INSERT INTO ProductImages (ProductId, ImageUrl, IsMain, DisplayOrder, CreatedAt)
SELECT
    Id,
    '/assets/images/product-img-' + CAST(((Id - 1) % 8 + 2) % 8 + 1 AS VARCHAR(10)) + '.jpg',
    0, 1, GETDATE()
FROM Products WHERE Id <= 50;
GO

INSERT INTO ProductImages (ProductId, ImageUrl, IsMain, DisplayOrder, CreatedAt)
SELECT
    Id,
    '/assets/images/product-img-' + CAST(((Id - 1) % 8 + 3) % 8 + 1 AS VARCHAR(10)) + '.jpg',
    0, 2, GETDATE()
FROM Products WHERE Id <= 30;
GO

-- =========================
-- DEFAULT ACCOUNTS (Admin & Test Customer)
-- Password is SHA256 hashed
-- =========================
IF NOT EXISTS (SELECT * FROM Account WHERE UserName = 'admin')
BEGIN
    INSERT INTO Account (UserName, FullName, Password, Email, Phone, Birthday, Status, Notes, CreatedAt, IsEmailVerified)
    VALUES ('admin', 'Administrator', 'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', 'admin@furnish.com', '0901234567', '1990-01-01', 1, 'Default admin account', GETDATE(), 1);
    PRINT 'Admin account created: admin / admin123';
END
ELSE
BEGIN
    -- Update existing admin password to hashed version and set email verified
    UPDATE Account SET Password = 'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', IsEmailVerified = 1 WHERE UserName = 'admin';
    PRINT 'Admin account updated with hashed password';
END
GO

IF NOT EXISTS (SELECT * FROM Account WHERE UserName = 'customer')
BEGIN
    INSERT INTO Account (UserName, FullName, Password, Email, Phone, Birthday, Status, Notes, CreatedAt, IsEmailVerified)
    VALUES ('customer', 'Test Customer', 'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', 'customer@furnish.com', '0909876543', '1995-05-15', 1, 'Test customer account', GETDATE(), 1);
    PRINT 'Customer account created: customer / customer123';
END
ELSE
BEGIN
    -- Update existing customer password to hashed version and set email verified
    UPDATE Account SET Password = 'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', IsEmailVerified = 1 WHERE UserName = 'customer';
    PRINT 'Customer account updated with hashed password';
END
GO

PRINT 'Seed data inserted successfully!';
GO
