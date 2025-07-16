

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View("Profile", EnvanterLib.getUserFromSession(this));
        }

        public IActionResult Profile()
        {
            return View("Profile", EnvanterLib.getUserFromSession(this));
        }

        public IActionResult Edit(string FirstName, string LastName, string Email)
        {
            Console.WriteLine(FirstName + "\n" + LastName + "\n" + Email);
            return View("Profile", EnvanterLib.getUserFromSession(this));
        }
        
    }
}
