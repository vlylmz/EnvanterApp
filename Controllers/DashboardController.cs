
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
