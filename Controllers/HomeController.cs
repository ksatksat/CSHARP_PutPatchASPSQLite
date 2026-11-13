using Microsoft.AspNetCore.Mvc;

namespace SimplePutPatchApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
