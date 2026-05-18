-- =============================================
-- Furnish Page Access Settings Patch
-- Adds admin-controlled open/closed page settings to an existing ShopDb.
-- Safe to run multiple times. Does not delete or reseed business data.
-- =============================================

USE ShopDb;
GO

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
    ALTER TABLE dbo.WebSettings ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_WebSettings_CreatedAt_PageAccess DEFAULT SYSUTCDATETIME();
GO

IF COL_LENGTH(N'dbo.WebSettings', N'UpdatedAt') IS NULL
    ALTER TABLE dbo.WebSettings ADD UpdatedAt DATETIME2 NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.WebSettings')
      AND name = N'IX_WebSettings_Key'
)
BEGIN
    CREATE UNIQUE INDEX IX_WebSettings_Key ON dbo.WebSettings(SettingKey);
END
GO

DECLARE @PageAccessSettings TABLE (
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(MAX) NULL
);

INSERT INTO @PageAccessSettings (SettingKey, SettingValue) VALUES
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
('PageCartOpenAt', ''),
('PageCheckoutOpenAt', ''),
('PageWishlistOpenAt', ''),
('PageOrdersOpenAt', ''),
('PageAboutOpenAt', ''),
('PageContactOpenAt', ''),
('PagePoliciesOpenAt', '');

INSERT INTO dbo.WebSettings (SettingKey, SettingValue, CreatedAt)
SELECT src.SettingKey, src.SettingValue, SYSUTCDATETIME()
FROM @PageAccessSettings src
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.WebSettings ws
    WHERE ws.SettingKey = src.SettingKey
);

UPDATE ws
SET
    ws.SettingValue = src.SettingValue,
    ws.UpdatedAt = SYSUTCDATETIME()
FROM dbo.WebSettings ws
INNER JOIN @PageAccessSettings src ON src.SettingKey = ws.SettingKey
WHERE ws.SettingValue IS NULL
   OR LTRIM(RTRIM(CONVERT(NVARCHAR(MAX), ws.SettingValue))) = '';
GO

PRINT 'Page access settings patch completed.';
GO
