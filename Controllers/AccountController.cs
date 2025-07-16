

using Microsoft.AspNetCore.Mvc;

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
            return View("Profile");
        }

        public IActionResult Edit(string FirstName, string LastName, string Email)
        {
            Console.WriteLine(FirstName + "\n" + LastName + "\n" + Email);
            return View("Profile");
        }
    }
}
