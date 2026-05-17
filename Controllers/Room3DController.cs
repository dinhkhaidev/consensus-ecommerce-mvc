using Microsoft.AspNetCore.Mvc;

namespace WebActionResults.Controllers;

public class Room3DController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Trải nghiệm phòng 3D";
        return View();
    }
}
