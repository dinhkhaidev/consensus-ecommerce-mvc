using Microsoft.AspNetCore.Mvc;
using WebActionResults.Models;

namespace WebActionResults.Controllers
{
    public class DemoController : Controller
    {
        private readonly ShopDbContext _context;
        public DemoController(ShopDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            //ViewBag.CategoryId = new
            //       SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }
        public IActionResult CheckDb()
        {
            bool canConnect = _context.Database.CanConnect();

            var connectCheck = canConnect ? "Connect" : "Not Connect";

            return Json(new
            {
                @checked = connectCheck
            });
        }

    }
}
