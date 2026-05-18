using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Repositories;
using WebActionResults.Data.Services;
using WebActionResults.Mappings;
using WebActionResults.Middleware;
using WebActionResults.Models;
using WebActionResults.Services;
using WebActionResults.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using VNPAY.Extensions;

// Load environment variables from .env file FIRST
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<WebSettingsActionFilter>();
});
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

// Activity Trace Log Service (Singleton - shared file lock)
builder.Services.AddSingleton<ITraceLogService, TraceLogService>();

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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost
});

app.UseHttpsRedirection();

var staticFileContentTypes = new FileExtensionContentTypeProvider();
staticFileContentTypes.Mappings[".gltf"] = "model/gltf+json";
staticFileContentTypes.Mappings[".glb"] = "model/gltf-binary";
staticFileContentTypes.Mappings[".fbx"] = "application/octet-stream";
staticFileContentTypes.Mappings[".bin"] = "application/octet-stream";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticFileContentTypes
});

app.UseMaintenanceMode();

app.UseRouting();

app.UseSession();

app.UsePageAccessControl();

app.UseSessionAuthentication();

app.UseActivityTrace();

app.UseAuthentication();
app.UseAuthorization();

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
