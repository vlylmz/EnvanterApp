using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
