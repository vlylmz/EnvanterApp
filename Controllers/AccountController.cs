using Microsoft.AspNetCore.Mvc;
using static WebApplication1.EnvanterLib;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View("Profile", GetUserFromSession(this));
        }


        public IActionResult Profile()
        {
            return View("Profile", GetUserFromSession(this));
        }


        [HttpPost]
        public IActionResult Edit(string FirstName, string LastName, string Email)
        {
            Console.WriteLine(FirstName + "\n" + LastName + "\n" + Email);
            return View("Profile", GetUserFromSession(this));
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login", new { key = "asdasd"});
        }
        
    }
}
