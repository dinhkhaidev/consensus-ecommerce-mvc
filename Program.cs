using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Repositories;
using WebActionResults.Data.Services;
using WebActionResults.Mappings;
using WebActionResults.Middleware;
using WebActionResults.Models;
using WebActionResults.Services;
using Microsoft.AspNetCore.Authentication;
using VNPAY.Extensions;

// Load environment variables from .env file FIRST
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".FurnishShop.Session";
});

// Read connection string from environment variable
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("ShopDbContext")
    ?? throw new InvalidOperationException("Connection string 'ShopDbContext' was not found.");

// DbContext
builder.Services.AddDbContext<ShopDbContext>(options => options.UseSqlServer(connectionString));

// Authentication - using session-based custom auth
builder.Services.AddAuthentication("Session")
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("Session", null);
builder.Services.AddAuthorization();

// HTTP Context Accessor for Cart Service and Authentication
builder.Services.AddHttpContextAccessor();

// Localization Service
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(AutoMapperProfile).Assembly);
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Services
builder.Services.AddScoped<WebActionResults.Data.Services.IAuthenticationService, WebActionResults.Data.Services.AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFulfillmentService, FulfillmentService>();
builder.Services.AddScoped<IWebSettingsService, WebSettingsService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Background Services
builder.Services.AddHostedService<TrackingBackgroundService>();

// VNPAY Configuration
builder.Services.AddVnpayClient(config =>
{
    config.TmnCode = Environment.GetEnvironmentVariable("VNPAY_TMNCODE") ?? "";
    config.HashSecret = Environment.GetEnvironmentVariable("VNPAY_HASH_SECRET") ?? "";
    config.CallbackUrl = Environment.GetEnvironmentVariable("VNPAY_CALLBACK_URL") ?? "";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseSessionAuthentication();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "adminRoot",
    pattern: "Admin",
    defaults: new { area = "Admin", controller = "Dashboard", action = "Index" });

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
