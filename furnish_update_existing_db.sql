-- =============================================
-- Furnish Existing Database Update Script
-- Safe patch for an existing ShopDb database.
-- This script does NOT delete or reseed business data.
-- Run this after pulling code updates when the current DB was created by an older schema.
-- =============================================

USE ShopDb;
GO

-- =============================================
-- Account
-- =============================================
IF OBJECT_ID(N'dbo.Account', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Account', N'FullName') IS NOT NULL
        ALTER TABLE dbo.Account ALTER COLUMN FullName NVARCHAR(100) NOT NULL;

    IF COL_LENGTH(N'dbo.Account', N'CreatedAt') IS NULL
        ALTER TABLE dbo.Account ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_Account_CreatedAt DEFAULT SYSUTCDATETIME();

    IF COL_LENGTH(N'dbo.Account', N'Role') IS NULL
        ALTER TABLE dbo.Account ADD Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Account_Role DEFAULT 'Customer';

    IF COL_LENGTH(N'dbo.Account', N'UpdatedAt') IS NULL
        ALTER TABLE dbo.Account ADD UpdatedAt DATETIME NULL;

    IF COL_LENGTH(N'dbo.Account', N'IsEmailVerified') IS NULL
        ALTER TABLE dbo.Account ADD IsEmailVerified BIT NULL;

    IF COL_LENGTH(N'dbo.Account', N'EmailVerificationToken') IS NULL
        ALTER TABLE dbo.Account ADD EmailVerificationToken NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.Account', N'EmailVerificationTokenExpiresAt') IS NULL
        ALTER TABLE dbo.Account ADD EmailVerificationTokenExpiresAt DATETIME NULL;

    IF COL_LENGTH(N'dbo.Account', N'AvatarUrl') IS NULL
        ALTER TABLE dbo.Account ADD AvatarUrl NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Account', N'CreatedAt') IS NOT NULL
    BEGIN
        UPDATE dbo.Account SET CreatedAt = SYSUTCDATETIME() WHERE CreatedAt IS NULL;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.default_constraints dc
            INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
            INNER JOIN sys.tables t ON t.object_id = c.object_id
            WHERE t.name = 'Account' AND c.name = 'CreatedAt'
        )
        BEGIN
            ALTER TABLE dbo.Account
            ADD CONSTRAINT DF_Account_CreatedAt DEFAULT SYSUTCDATETIME() FOR CreatedAt;
        END
    END

    UPDATE dbo.Account SET Role = 'Customer' WHERE Role IS NULL OR LTRIM(RTRIM(Role)) = '';
    UPDATE dbo.Account SET Role = 'Admin' WHERE LOWER(UserName) = 'admin';
    UPDATE dbo.Account SET IsEmailVerified = 0 WHERE IsEmailVerified IS NULL;
END
GO

-- =============================================
-- Product variants / images
-- =============================================
IF OBJECT_ID(N'dbo.ProductVariants', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.ProductVariants', N'SKU') IS NOT NULL
        ALTER TABLE dbo.ProductVariants ALTER COLUMN SKU NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.ProductVariants', N'IsActive') IS NULL
        ALTER TABLE dbo.ProductVariants ADD IsActive BIT NOT NULL CONSTRAINT DF_ProductVariants_IsActive DEFAULT 1;

    IF COL_LENGTH(N'dbo.ProductVariants', N'CreatedAt') IS NULL
        ALTER TABLE dbo.ProductVariants ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductVariants_CreatedAt DEFAULT SYSUTCDATETIME();
END
GO

IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.ProductImages', N'VariantId') IS NULL
        ALTER TABLE dbo.ProductImages ADD VariantId INT NULL;

    IF COL_LENGTH(N'dbo.ProductImages', N'AltText') IS NULL
        ALTER TABLE dbo.ProductImages ADD AltText NVARCHAR(255) NULL;

    IF COL_LENGTH(N'dbo.ProductImages', N'DisplayOrder') IS NULL
        ALTER TABLE dbo.ProductImages ADD DisplayOrder INT NOT NULL CONSTRAINT DF_ProductImages_DisplayOrder DEFAULT 0;

    IF COL_LENGTH(N'dbo.ProductImages', N'IsMain') IS NULL
        ALTER TABLE dbo.ProductImages ADD IsMain BIT NOT NULL CONSTRAINT DF_ProductImages_IsMain DEFAULT 0;

    IF COL_LENGTH(N'dbo.ProductImages', N'CreatedAt') IS NULL
        ALTER TABLE dbo.ProductImages ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductImages_CreatedAt DEFAULT SYSUTCDATETIME();

    IF COL_LENGTH(N'dbo.ProductImages', N'ImageUrl') IS NOT NULL
    BEGIN
        UPDATE dbo.ProductImages SET ImageUrl = '' WHERE ImageUrl IS NULL;
        ALTER TABLE dbo.ProductImages ALTER COLUMN ImageUrl NVARCHAR(500) NOT NULL;
    END
END
GO

-- Keep product cleanup predictable. Drop both ProductImages FKs first, then
-- recreate VariantId as NO ACTION before ProductId CASCADE to avoid SQL Server
-- multiple cascade path detection on existing databases.
IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NOT NULL
BEGIN
    DECLARE @fkProductImagesProduct NVARCHAR(128);
    DECLARE @fkProductImagesVariant NVARCHAR(128);

    SELECT @fkProductImagesProduct = fk.name
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    INNER JOIN sys.tables pt ON pt.object_id = fk.parent_object_id
    INNER JOIN sys.columns pc ON pc.object_id = pt.object_id AND pc.column_id = fkc.parent_column_id
    INNER JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
    WHERE pt.name = 'ProductImages' AND pc.name = 'ProductId' AND rt.name = 'Products';

    SELECT @fkProductImagesVariant = fk.name
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    INNER JOIN sys.tables pt ON pt.object_id = fk.parent_object_id
    INNER JOIN sys.columns pc ON pc.object_id = pt.object_id AND pc.column_id = fkc.parent_column_id
    INNER JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
    WHERE pt.name = 'ProductImages' AND pc.name = 'VariantId' AND rt.name = 'ProductVariants';

    IF @fkProductImagesVariant IS NOT NULL
        EXEC(N'ALTER TABLE dbo.ProductImages DROP CONSTRAINT [' + @fkProductImagesVariant + N']');

    IF @fkProductImagesProduct IS NOT NULL
        EXEC(N'ALTER TABLE dbo.ProductImages DROP CONSTRAINT [' + @fkProductImagesProduct + N']');
END
GO

IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
BEGIN
    DELETE pi
    FROM dbo.ProductImages pi
    LEFT JOIN dbo.Products p ON p.Id = pi.ProductId
    WHERE p.Id IS NULL;
END
GO

IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.ProductVariants', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.ProductImages', N'VariantId') IS NOT NULL
BEGIN
    UPDATE pi
    SET VariantId = NULL
    FROM dbo.ProductImages pi
    LEFT JOIN dbo.ProductVariants v ON v.Id = pi.VariantId
    WHERE pi.VariantId IS NOT NULL AND v.Id IS NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductImages_ProductVariants_VariantId'
    )
    BEGIN
        ALTER TABLE dbo.ProductImages WITH CHECK ADD CONSTRAINT FK_ProductImages_ProductVariants_VariantId
            FOREIGN KEY (VariantId) REFERENCES dbo.ProductVariants(Id) ON DELETE NO ACTION;
    END
END
GO

IF OBJECT_ID(N'dbo.ProductImages', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ProductImages_Products_ProductId'
    )
    BEGIN
        ALTER TABLE dbo.ProductImages WITH CHECK ADD CONSTRAINT FK_ProductImages_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE;
    END
END
GO

-- =============================================
-- Carts / cart items
-- =============================================
IF OBJECT_ID(N'dbo.Carts', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Carts', N'CreatedAt') IS NULL
        ALTER TABLE dbo.Carts ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_Carts_CreatedAt DEFAULT SYSUTCDATETIME();

    IF COL_LENGTH(N'dbo.Carts', N'UpdatedAt') IS NULL
        ALTER TABLE dbo.Carts ADD UpdatedAt DATETIME NOT NULL CONSTRAINT DF_Carts_UpdatedAt DEFAULT SYSUTCDATETIME();
END
GO

-- Preserve order history when an account is removed. Admin delete now deactivates
-- accounts with orders instead of hard-deleting them.
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Account', N'U') IS NOT NULL
BEGIN
    DECLARE @fkOrdersAccount NVARCHAR(128);
    SELECT @fkOrdersAccount = fk.name
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    INNER JOIN sys.tables pt ON pt.object_id = fk.parent_object_id
    INNER JOIN sys.columns pc ON pc.object_id = pt.object_id AND pc.column_id = fkc.parent_column_id
    INNER JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
    WHERE pt.name = 'Orders' AND pc.name = 'UserId' AND rt.name = 'Account';

    IF @fkOrdersAccount IS NOT NULL
        EXEC(N'ALTER TABLE dbo.Orders DROP CONSTRAINT [' + @fkOrdersAccount + N']');

    ALTER TABLE dbo.Orders WITH CHECK ADD CONSTRAINT FK_Orders_Account_UserId
        FOREIGN KEY (UserId) REFERENCES dbo.Account(Id) ON DELETE NO ACTION;
END
GO

IF OBJECT_ID(N'dbo.CartItems', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.CartItems', N'VariantId') IS NULL
        ALTER TABLE dbo.CartItems ADD VariantId INT NULL;

    IF COL_LENGTH(N'dbo.CartItems', N'ProductName') IS NULL
        ALTER TABLE dbo.CartItems ADD ProductName NVARCHAR(150) NOT NULL CONSTRAINT DF_CartItems_ProductName DEFAULT '';

    IF COL_LENGTH(N'dbo.CartItems', N'VariantName') IS NULL
        ALTER TABLE dbo.CartItems ADD VariantName NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.CartItems', N'ImageUrl') IS NULL
        ALTER TABLE dbo.CartItems ADD ImageUrl NVARCHAR(255) NULL;

    IF COL_LENGTH(N'dbo.CartItems', N'BasePrice') IS NULL
        ALTER TABLE dbo.CartItems ADD BasePrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_CartItems_BasePrice DEFAULT 0;

    IF COL_LENGTH(N'dbo.CartItems', N'PriceAdjustment') IS NULL
        ALTER TABLE dbo.CartItems ADD PriceAdjustment DECIMAL(18,2) NOT NULL CONSTRAINT DF_CartItems_PriceAdjustment DEFAULT 0;

    IF COL_LENGTH(N'dbo.CartItems', N'PriceBreakdown') IS NULL
        ALTER TABLE dbo.CartItems ADD PriceBreakdown NVARCHAR(255) NULL;

    UPDATE dbo.CartItems SET BasePrice = 0 WHERE BasePrice IS NULL;
    UPDATE dbo.CartItems SET PriceAdjustment = 0 WHERE PriceAdjustment IS NULL;
    ALTER TABLE dbo.CartItems ALTER COLUMN BasePrice DECIMAL(18,2) NOT NULL;
    ALTER TABLE dbo.CartItems ALTER COLUMN PriceAdjustment DECIMAL(18,2) NOT NULL;
END
GO

-- =============================================
-- Orders / order items
-- =============================================
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Orders', N'SubTotal') IS NULL
        ALTER TABLE dbo.Orders ADD SubTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Orders_SubTotal DEFAULT 0;

    IF COL_LENGTH(N'dbo.Orders', N'DiscountAmount') IS NULL
        ALTER TABLE dbo.Orders ADD DiscountAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Orders_DiscountAmount DEFAULT 0;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingFee') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingFee DECIMAL(18,2) NOT NULL CONSTRAINT DF_Orders_ShippingFee DEFAULT 0;

    IF COL_LENGTH(N'dbo.Orders', N'TotalAmount') IS NULL
        ALTER TABLE dbo.Orders ADD TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Orders_TotalAmount DEFAULT 0;

    IF COL_LENGTH(N'dbo.Orders', N'AddressId') IS NULL
        ALTER TABLE dbo.Orders ADD AddressId INT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingName') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingName NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingPhone') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingPhone NVARCHAR(20) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingAddress') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingAddress NVARCHAR(255) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingCity') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingCity NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingDistrict') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingDistrict NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippingWard') IS NULL
        ALTER TABLE dbo.Orders ADD ShippingWard NVARCHAR(100) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'Notes') IS NULL
        ALTER TABLE dbo.Orders ADD Notes NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'PaymentMethod') IS NULL
        ALTER TABLE dbo.Orders ADD PaymentMethod INT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'PaymentStatus') IS NULL
        ALTER TABLE dbo.Orders ADD PaymentStatus INT NOT NULL CONSTRAINT DF_Orders_PaymentStatus DEFAULT 0;

    IF COL_LENGTH(N'dbo.Orders', N'CouponId') IS NULL
        ALTER TABLE dbo.Orders ADD CouponId INT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'UpdatedAt') IS NULL
        ALTER TABLE dbo.Orders ADD UpdatedAt DATETIME NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ShippedAt') IS NULL
        ALTER TABLE dbo.Orders ADD ShippedAt DATETIME NULL;

    IF COL_LENGTH(N'dbo.Orders', N'DeliveredAt') IS NULL
        ALTER TABLE dbo.Orders ADD DeliveredAt DATETIME NULL;

    IF COL_LENGTH(N'dbo.Orders', N'CancelReason') IS NULL
        ALTER TABLE dbo.Orders ADD CancelReason NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'CancelRequestedAt') IS NULL
        ALTER TABLE dbo.Orders ADD CancelRequestedAt DATETIME2 NULL;

    IF COL_LENGTH(N'dbo.Orders', N'CancelRequestedFromStatus') IS NULL
        ALTER TABLE dbo.Orders ADD CancelRequestedFromStatus INT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'CancelApproved') IS NULL
        ALTER TABLE dbo.Orders ADD CancelApproved BIT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'CancelAdminNote') IS NULL
        ALTER TABLE dbo.Orders ADD CancelAdminNote NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'CancelReviewedAt') IS NULL
        ALTER TABLE dbo.Orders ADD CancelReviewedAt DATETIME2 NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnReason') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnReason NVARCHAR(1000) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnImageUrl') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnImageUrl NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnRequestedAt') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnRequestedAt DATETIME2 NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnRequestedFromStatus') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnRequestedFromStatus INT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnApproved') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnApproved BIT NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnAdminNote') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnAdminNote NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Orders', N'ReturnReviewedAt') IS NULL
        ALTER TABLE dbo.Orders ADD ReturnReviewedAt DATETIME2 NULL;
END
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.OrderItems', N'VariantId') IS NULL
        ALTER TABLE dbo.OrderItems ADD VariantId INT NULL;

    IF COL_LENGTH(N'dbo.OrderItems', N'VariantName') IS NULL
        ALTER TABLE dbo.OrderItems ADD VariantName NVARCHAR(50) NULL;

    IF COL_LENGTH(N'dbo.OrderItems', N'TotalPrice') IS NULL
        ALTER TABLE dbo.OrderItems ADD TotalPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_OrderItems_TotalPrice DEFAULT 0;
END
GO

-- =============================================
-- Payments / reviews / shipments
-- =============================================
IF OBJECT_ID(N'dbo.Payments', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Payments', N'Method') IS NOT NULL
    BEGIN
        UPDATE dbo.Payments SET Method = 'COD' WHERE Method IS NULL OR LTRIM(RTRIM(Method)) = '';
        ALTER TABLE dbo.Payments ALTER COLUMN Method NVARCHAR(50) NOT NULL;
    END

    IF COL_LENGTH(N'dbo.Payments', N'Status') IS NOT NULL
    BEGIN
        UPDATE dbo.Payments SET Status = 'Pending' WHERE Status IS NULL OR LTRIM(RTRIM(Status)) = '';
        ALTER TABLE dbo.Payments ALTER COLUMN Status NVARCHAR(50) NOT NULL;
    END

    IF COL_LENGTH(N'dbo.Payments', N'PaymentUrl') IS NULL
        ALTER TABLE dbo.Payments ADD PaymentUrl NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Payments', N'ReturnUrl') IS NULL
        ALTER TABLE dbo.Payments ADD ReturnUrl NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.Payments', N'ErrorMessage') IS NULL
        ALTER TABLE dbo.Payments ADD ErrorMessage NVARCHAR(255) NULL;

    IF COL_LENGTH(N'dbo.Payments', N'CompletedAt') IS NULL
        ALTER TABLE dbo.Payments ADD CompletedAt DATETIME2 NULL;

    IF COL_LENGTH(N'dbo.Payments', N'ExpiresAt') IS NULL
        ALTER TABLE dbo.Payments ADD ExpiresAt DATETIME2 NULL;
END
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Reviews', N'IsApproved') IS NULL
        ALTER TABLE dbo.Reviews ADD IsApproved BIT NOT NULL CONSTRAINT DF_Reviews_IsApproved DEFAULT 0;

    IF COL_LENGTH(N'dbo.Reviews', N'UpdatedAt') IS NULL
        ALTER TABLE dbo.Reviews ADD UpdatedAt DATETIME2 NULL;
END
GO

IF OBJECT_ID(N'dbo.Shipments', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.Shipments', N'Carrier') IS NOT NULL
    BEGIN
        UPDATE dbo.Shipments SET Carrier = '' WHERE Carrier IS NULL;
        ALTER TABLE dbo.Shipments ALTER COLUMN Carrier NVARCHAR(100) NOT NULL;
    END

    IF COL_LENGTH(N'dbo.Shipments', N'Status') IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM sys.columns c
            INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID(N'dbo.Shipments')
              AND c.name = N'Status'
              AND t.name IN (N'nvarchar', N'varchar', N'nchar', N'char')
        )
        BEGIN
            IF COL_LENGTH(N'dbo.Shipments', N'StatusInt') IS NULL
                ALTER TABLE dbo.Shipments ADD StatusInt INT NOT NULL CONSTRAINT DF_Shipments_StatusInt DEFAULT 0;

            UPDATE dbo.Shipments
            SET StatusInt = COALESCE(
                TRY_CONVERT(INT, Status),
                CASE LOWER(LTRIM(RTRIM(Status)))
                    WHEN 'pickedup' THEN 1
                    WHEN 'picked_up' THEN 1
                    WHEN 'intransit' THEN 2
                    WHEN 'in_transit' THEN 2
                    WHEN 'outfordelivery' THEN 3
                    WHEN 'out_for_delivery' THEN 3
                    WHEN 'delivered' THEN 4
                    WHEN 'failed' THEN 5
                    WHEN 'returned' THEN 6
                    ELSE 0
                END
            );

            ALTER TABLE dbo.Shipments DROP COLUMN Status;
            EXEC sp_rename N'dbo.Shipments.StatusInt', N'Status', N'COLUMN';
        END
        ELSE
        BEGIN
            UPDATE dbo.Shipments SET Status = 0 WHERE Status IS NULL;
            ALTER TABLE dbo.Shipments ALTER COLUMN Status INT NOT NULL;
        END
    END

    IF COL_LENGTH(N'dbo.Shipments', N'UpdatedAt') IS NULL
        ALTER TABLE dbo.Shipments ADD UpdatedAt DATETIME2 NULL;
END
GO

-- =============================================
-- WebSettings: admin configurable site settings
-- =============================================
IF OBJECT_ID(N'dbo.WebSettings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.WebSettings (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WebSettings PRIMARY KEY,
        SettingKey NVARCHAR(100) NOT NULL,
        SettingValue NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WebSettings_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL
    );

    CREATE UNIQUE INDEX IX_WebSettings_Key ON dbo.WebSettings(SettingKey);
END
GO

IF COL_LENGTH(N'dbo.WebSettings', N'CreatedAt') IS NULL
    ALTER TABLE dbo.WebSettings ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WebSettings_CreatedAt DEFAULT SYSUTCDATETIME();
GO

IF COL_LENGTH(N'dbo.WebSettings', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.WebSettings ADD UpdatedAt DATETIME2 NULL;
GO

IF COL_LENGTH(N'dbo.WebSettings', N'SettingValue') IS NOT NULL
    ALTER TABLE dbo.WebSettings ALTER COLUMN SettingValue NVARCHAR(MAX) NULL;
GO

IF OBJECT_ID(N'dbo.WebSettings', N'U') IS NOT NULL
BEGIN
    ;WITH RankedSettings AS (
        SELECT
            Id,
            ROW_NUMBER() OVER (
                PARTITION BY SettingKey
                ORDER BY
                    CASE WHEN SettingValue IS NOT NULL AND LTRIM(RTRIM(CONVERT(NVARCHAR(MAX), SettingValue))) <> '' THEN 0 ELSE 1 END,
                    UpdatedAt DESC,
                    CreatedAt DESC,
                    Id DESC
            ) AS RowNumber
        FROM dbo.WebSettings
    )
    DELETE FROM RankedSettings
    WHERE RowNumber > 1;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.WebSettings')
          AND name = N'IX_WebSettings_Key'
    )
    BEGIN
        CREATE UNIQUE INDEX IX_WebSettings_Key ON dbo.WebSettings(SettingKey);
    END
END
GO

DECLARE @DefaultSettings TABLE (
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NULL
);

INSERT INTO @DefaultSettings (SettingKey, SettingValue) VALUES
('StoreName', 'Furnish'),
('SiteTitle', 'Furnish - Premium Furniture Store'),
('SiteTagline', 'Premium Furniture'),
('SiteDescription', 'Discover premium quality furniture for every room in your home.'),
('LogoUrl', ''),
('FaviconUrl', ''),
('PrimaryColor', '#d47f31'),
('AccentColor', '#1f2933'),
('DefaultLanguage', 'vi'),
('SeoKeywords', 'furniture, sofa, chair, table, home decor, noi that'),
('OpenGraphImageUrl', ''),
('AdminConsoleTitle', 'Furnish Admin'),
('AdminDashboardTitle', 'Dashboard'),
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
('ContactEmail', 'contact@furnish.com'),
('SupportEmail', 'support@furnish.com'),
('SalesEmail', 'sales@furnish.com'),
('ContactPhone', '0901234567'),
('Hotline', '1900 1234'),
('ZaloPhone', '0901234567'),
('Address', '123 Furniture Street, District 1, Ho Chi Minh City'),
('WorkingHours', '08:00 - 21:00'),
('BusinessTaxCode', ''),
('FooterShortAbout', 'Premium furniture for modern homes.'),
('FooterCopyright', '(c) 2024 Furnish. All rights reserved.'),
('FacebookUrl', 'https://facebook.com/furnish'),
('InstagramUrl', 'https://instagram.com/furnish'),
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
('HideClosedPageLinks', 'false'),
('ClosedPageEyebrow', N'Khu vực đặc biệt'),
('ClosedPageTitle', N'Khu vực này đang tạm khóa'),
('ClosedPageMessage', N'Quản trị viên đang giữ tính năng này ở chế độ bí mật. Vui lòng quay lại sau.'),
('ClosedPageButtonText', N'Về trang chủ'),
('ClosedPageBackgroundImageUrl', '/assets/images/sontungmtpmv.png'),
('PageHomeEnabled', 'true'),
('PageShopEnabled', 'true'),
('PageCategoriesEnabled', 'true'),
('PageRoom3DEnabled', 'true'),
('PageAiStylistEnabled', 'true'),
('PageCartEnabled', 'true'),
('PageCheckoutEnabled', 'true'),
('PageWishlistEnabled', 'true'),
('PageOrdersEnabled', 'true'),
('PageAboutEnabled', 'true'),
('PageContactEnabled', 'true'),
('PagePoliciesEnabled', 'true'),
('PageHomeOpenAt', ''),
('PageShopOpenAt', ''),
('PageCategoriesOpenAt', ''),
('PageRoom3DOpenAt', ''),
('PageAiStylistOpenAt', ''),
('PageCartOpenAt', ''),
('PageCheckoutOpenAt', ''),
('PageWishlistOpenAt', ''),
('PageOrdersOpenAt', ''),
('PageAboutOpenAt', ''),
('PageContactOpenAt', ''),
('PagePoliciesOpenAt', ''),
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

INSERT INTO dbo.WebSettings (SettingKey, SettingValue, CreatedAt)
SELECT ds.SettingKey, ds.SettingValue, SYSUTCDATETIME()
FROM @DefaultSettings ds
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.WebSettings ws
    WHERE ws.SettingKey = ds.SettingKey
);
GO

-- =============================================
-- EF migration history sync
-- =============================================
IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.__EFMigrationsHistory (
        MigrationId NVARCHAR(150) NOT NULL CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY,
        ProductVersion NVARCHAR(32) NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20260510125916_InitialCreate')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260510125916_InitialCreate', '8.0.25');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20260510140146_AddCartItemPriceColumns')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260510140146_AddCartItemPriceColumns', '8.0.25');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20260515123000_AddOrderRequestFlow')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260515123000_AddOrderRequestFlow', '8.0.25');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20260517062015_AddAvatarUrl')
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260517062015_AddAvatarUrl', '8.0.25');
GO

PRINT 'Furnish existing database update completed successfully.';
GO
