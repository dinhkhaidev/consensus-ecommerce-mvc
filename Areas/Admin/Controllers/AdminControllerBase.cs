using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminControllerBase : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var userRole = HttpContext.Session.GetString("USER_ROLE");
        if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
        }
    }
}
