using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Services;
using WebActionResults.Models;
using WebActionResults.ViewModels;

namespace WebActionResults.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICatalogService _catalogService;
        private readonly IWebSettingsService _webSettingsService;
        private readonly ShopDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ICatalogService catalogService,
            IWebSettingsService webSettingsService,
            ShopDbContext context)
        {
            _logger = logger;
            _catalogService = catalogService;
            _webSettingsService = webSettingsService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _catalogService.GetFeaturedProductsAsync(6);
            var categories = await _catalogService.GetCategoriesWithProductsAsync();
            var settings = await _webSettingsService.GetAllSettingsAsync();
            var testimonials = await GetHomepageTestimonialsAsync();

            ViewData["FeaturedProducts"] = featuredProducts;
            ViewData["Categories"] = categories;
            ViewData["WebSettings"] = settings;
            ViewData["Testimonials"] = testimonials;

            return View();
        }

        private async Task<List<ReviewViewModel>> GetHomepageTestimonialsAsync()
        {
            var testimonials = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.IsApproved && !string.IsNullOrWhiteSpace(r.Comment))
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(3)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    UserName = r.User.FullName ?? r.User.UserName,
                    UserAvatar = r.User.AvatarUrl,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var fallbacks = new List<ReviewViewModel>
            {
                new()
                {
                    UserName = "Chị Mai",
                    Comment = "Bao nhiêu nồi bánh chưng rồi mới gặp được shop mà chủ vừa đẹp trai, có gu, lại bán hàng uy tín đến vậy.",
                    Rating = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    UserName = "Chị Linh",
                    Comment = "Mua bộ sofa mà được tư vấn như làm thiết kế riêng. Chủ shop có tâm có tầm, chúc shop luôn đông khách.",
                    Rating = 5,
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    UserName = "Bạn Ngọc",
                    Comment = "Đồ ngoài đời đẹp hơn ảnh, giao nhanh, đóng gói kỹ. Chủ shop đẹp trai mà còn nói chuyện dễ thương nữa.",
                    Rating = 5,
                    CreatedAt = DateTime.UtcNow
                }
            };

            if (!testimonials.Any())
            {
                return fallbacks;
            }

            return testimonials.Take(1).Concat(fallbacks.Take(2)).ToList();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult ShippingPolicy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
