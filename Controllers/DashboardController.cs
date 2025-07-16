using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;



namespace WebApplication1.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ILogger<DashboardController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (getUserFromSession() == null)
                return RedirectToAction("Index", "Login");

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private User? getUserFromSession()
        {
            var userJson = HttpContext.Session.GetString("userJson");
            if (userJson == null || string.IsNullOrEmpty(userJson))
                return null;
            return JsonSerializer.Deserialize<User>(userJson);
        }
    }
}
