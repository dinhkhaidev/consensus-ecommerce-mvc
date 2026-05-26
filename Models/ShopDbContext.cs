using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;

namespace WebActionResults.Models;

public partial class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }
    public virtual DbSet<ProductVariant> ProductVariants { get; set; }
    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Wishlist> Wishlists { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Coupon> Coupons { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Shipment> Shipments { get; set; }
    public virtual DbSet<WebSettings> WebSettings { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");
            entity.Property(e => e.Birthday).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.Email).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(150);
            entity.Property(e => e.Password).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Phone).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.UserName).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.IsEmailVerified)
                .HasColumnName("IsEmailVerified")
                .IsRequired(false);
            entity.Property(e => e.EmailVerificationToken).HasMaxLength(100);
            entity.Property(e => e.EmailVerificationTokenExpiresAt).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("Customer");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.QuantityPerUnit).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryID)
                .HasPrincipalKey(c => c.Id)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierID)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers");
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Phone).HasMaxLength(30).IsUnicode(false);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AddressLine).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Ward).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("ProductVariants");
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.SKU).HasMaxLength(100);
            entity.Property(e => e.PriceAdjustment).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.AltText).HasMaxLength(255);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Variant)
                .WithMany(v => v.Images)
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.Property(e => e.Comment).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.ToTable("Wishlists");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.Property(e => e.OrderNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingFee).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ShippingName).HasMaxLength(100);
            entity.Property(e => e.ShippingPhone).HasMaxLength(20);
            entity.Property(e => e.ShippingAddress).HasMaxLength(255);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingDistrict).HasMaxLength(100);
            entity.Property(e => e.ShippingWard).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CancelReason).HasMaxLength(500);
            entity.Property(e => e.CancelAdminNote).HasMaxLength(500);
            entity.Property(e => e.ReturnReason).HasMaxLength(1000);
            entity.Property(e => e.ReturnImageUrl).HasMaxLength(500);
            entity.Property(e => e.ReturnAdminNote).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Address)
                .WithMany()
                .HasForeignKey(e => e.AddressId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Coupon)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.Property(e => e.ProductName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.VariantName).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.PaymentUrl).HasMaxLength(500);
            entity.Property(e => e.ReturnUrl).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(255);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("Shipments");
            entity.Property(e => e.Carrier).HasMaxLength(100);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TrackingUrl).HasMaxLength(500);
            entity.Property(e => e.LastUpdate).HasMaxLength(500);
            entity.Property(e => e.EstimatedDelivery).HasMaxLength(500);
            entity.Property(e => e.ActualDelivery).HasMaxLength(500);

            entity.HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WebSettings>(entity =>
        {
            entity.ToTable("WebSettings");
            entity.Property(e => e.SettingKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SettingValue).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.VariantName).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);

            entity.HasOne(e => e.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ProductVariant>()
                .WithMany()
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
