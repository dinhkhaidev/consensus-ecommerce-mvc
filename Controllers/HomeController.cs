using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebActionResults.Data.Services;
using WebActionResults.Models;

namespace WebActionResults.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICatalogService _catalogService;
        private readonly IWebSettingsService _webSettingsService;

        public HomeController(ILogger<HomeController> logger, ICatalogService catalogService, IWebSettingsService webSettingsService)
        {
            _logger = logger;
            _catalogService = catalogService;
            _webSettingsService = webSettingsService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _catalogService.GetFeaturedProductsAsync(6);
            var categories = await _catalogService.GetCategoriesWithProductsAsync();
            var settings = await _webSettingsService.GetAllSettingsAsync();

            ViewData["FeaturedProducts"] = featuredProducts;
            ViewData["Categories"] = categories;
            ViewData["WebSettings"] = settings;

            return View();
        }

        public IActionResult Privacy()
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