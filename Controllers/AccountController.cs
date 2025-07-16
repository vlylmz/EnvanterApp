

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View("Profile");
        }

        public IActionResult Profile()
        {
            return View("Profile", getUserFromSession());
        }

        public IActionResult Edit(string FirstName, string LastName, string Email)
        {
            Console.WriteLine(FirstName + "\n" + LastName + "\n" + Email);
            return View("Profile");
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
