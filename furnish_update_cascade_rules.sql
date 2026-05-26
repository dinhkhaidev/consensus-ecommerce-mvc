-- =============================================
-- Furnish Cascade Rules Update
-- Safe patch for existing databases.
-- Keeps product cleanup predictable and preserves order history.
-- =============================================

USE ShopDb;
GO

-- ProductImages currently may have a VariantId FK with SET NULL/CASCADE.
-- Drop both ProductImages FKs first, then recreate them in an order that avoids
-- SQL Server multiple cascade path detection.
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

-- Do not cascade-delete order history when an account is removed.
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
