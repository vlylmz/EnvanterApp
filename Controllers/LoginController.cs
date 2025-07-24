using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using OtpNet;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class LoginController(AppDbContext context) : Controller
    {
        readonly AppDbContext context = context;
        private static byte[]? adminTotpSecret;

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("user") == "admin")
                return RedirectToAction("Index", "Home");
            return View();
        }


        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            Console.WriteLine("username:" + username + " password:" + password);
            if (username == "admin" && password == "1234")
            {
                if (HttpContext.Session.GetString("adminTotpSecretNew") != null)
                {
                    Console.WriteLine("adminTotpSecretNew isn't null, redirecting");
                    TempData["allowRedirectToQrRegister"] = true;
                    return RedirectToAction("QrRegister");
                }

                if (adminTotpSecret == null)
                {
                    Console.WriteLine("adminTotpSecret is null, redirecting to qrregister");
                    HttpContext.Session.SetString("adminTotpSecretNew", Convert.ToBase64String(KeyGeneration.GenerateRandomKey(16)));
                    TempData["allowRedirectToQrRegister"] = true;
                    return RedirectToAction("QrRegister");
                }

                TempData["allowRedirectToQrValidate"] = true;
                return RedirectToAction("QrValidate", adminTotpSecret);
            }

            ViewBag.Error = "Wrong username or password";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult QrRegister(string code)
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
                return RedirectToAction("Index", "Home");


            Console.WriteLine("register");
            var sessionBase64Totp = HttpContext.Session.GetString("adminTotpSecretNew");

            if (sessionBase64Totp == null)
            {
                Console.WriteLine("null");
                ViewBag.Error = "new secret is null";
                return RedirectToAction("Index");
            }

            var sessionTotp = Convert.FromBase64String(sessionBase64Totp);
            var trueTotp = new Totp(sessionTotp).ComputeTotp();
            if (code == trueTotp)
            {
                Console.WriteLine("ok");
                adminTotpSecret = sessionTotp;
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("user", "admin");
                return RedirectToAction("Index", "Home");
            }

            else
            {
                Console.WriteLine("wrong");
                ViewBag.Error = "wrong 2FA code, true was: " + trueTotp + " you entered: " + code;
                return View();
            }
        }


        [HttpPost]
        public IActionResult QrValidate(string code)
        {
            Console.WriteLine("qrvalidate code");
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
                return RedirectToAction("Index", "Home");


            var trueTotp = new Totp(adminTotpSecret).ComputeTotp();
            if (code == trueTotp)
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("user", "admin");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "wrong 2FA code, right was: " + trueTotp;
            return View();
        }

        public IActionResult QrRegister()
        {
            var key = TempData["allowRedirectToQrRegister"];
            if (key == null)
                return RedirectToAction("Index");

            return View("QrRegister");
        }

        public IActionResult QrValidate()
        {
            Console.WriteLine("qrvalidate private");
            var key = TempData["allowRedirectToQrValidate"];
            if (key == null)
                return RedirectToAction("Index");

            return View("QrValidate");
        }


        public IActionResult QrSkip()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("user", "admin");
            return RedirectToAction("Index");
        }

    }
}