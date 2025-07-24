
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController() : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
