using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using OtpNet;
using WebApplication1.Data;
using WebApplication1.EnvanterLib;

namespace WebApplication1.Controllers
{

    public class LoginController(AppDbContext context) : Controller
    {
        readonly AppDbContext context = context;

        public IActionResult Index()
        {
            if (this.GetUserFromHttpContext() != null)
            {
                Console.WriteLine("user is already logged in, redirecting to home");
                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public IActionResult Index(string email, string password)
        {
            Console.WriteLine("email: " + email + " password: " + password);

            var userResult = context.ApplicationUsers.Where(u =>
                         u.Email == email &&
                         u.Password == password
                         ).ToList().FirstOrDefault();

            if (userResult != null)
            {
                if (userResult.TotpSecret != null)
                {
                    Console.WriteLine("user has 2FA enabled, redirecting to qrvalidate");
                    this.TwoFactorHoldUser(userResult);
                    return View("QrValidate");
                }
                else
                {
                    Console.WriteLine("user has no 2FA enabled, logging in directly");
                    this.SaveUserToHttpContext(userResult);
                    return RedirectToAction("Index", "Home");
                }
            }

            Console.WriteLine("user with specified password not found, returning to login");
            ViewBag.Error = "Wrong email or password";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        [Route("QrRegister")]
        [HttpPost]
        public IActionResult QrRegister(string code)
        {
            Console.WriteLine("qrregister code: " + code);
            
            var loggedInUser = this.GetUserFromHttpContext();
            if (loggedInUser == null)
            {
                Console.WriteLine("no user found in session, redirecting to index");
                return RedirectToAction("Index");
            }
            var TotpSecretRegisterKey = HttpContext.Session.Get("TotpSecretRegisterKey");

            var trueTotp = new Totp(TotpSecretRegisterKey).ComputeTotp();
            if (code == trueTotp)
            {
                Console.WriteLine("2fa ok");
                loggedInUser.TotpSecret = TotpSecretRegisterKey;
                this.SaveUserToHttpContext(loggedInUser);
                context.Update(loggedInUser);
                context.SaveChanges();
                HttpContext.Session.Remove("TotpSecretRegisterKey");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                Console.WriteLine("2fa wrong");
                ViewBag.Error = "wrong 2FA code, true was: " + trueTotp + " you entered: " + code;
                return View(TotpSecretRegisterKey);
            }
        }


        [HttpPost]
        public IActionResult QrValidate(string code)
        {
            Console.WriteLine("qrvalidate code: " + code);
            if (CheckIfAlreadyLoggedIn())
                return RedirectToAction("Index", "Home");

            var user = this.GetTwoFactorHoldUser();
            if (user == null)
            {
                Console.WriteLine("no user found in 2fa hold, redirecting to index");
                return RedirectToAction("Index");
            }

            var trueTotp = new Totp(user.TotpSecret).ComputeTotp();
            if (code == trueTotp)
            {
                this.SaveUserToHttpContext(user);
                this.RemoveTwoFactorHoldUser();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "wrong 2FA code, right was: " + trueTotp;
            return View();
        }


        public IActionResult QrValidate()
        {
            if (CheckIfAlreadyLoggedIn())
                return RedirectToAction("Index", "Home");
            
            var user = this.GetTwoFactorHoldUser();
            if (user == null)
            {
                Console.WriteLine("no user found in 2fa hold, redirecting to index");
                return RedirectToAction("Index");
            }

            return View();
        }


        [Route("QrRegister")]
        public IActionResult QrRegister()
        {
            var loggedInUser = this.GetUserFromHttpContext();

            if (loggedInUser == null)
            {
                Console.WriteLine("no user found in session, redirecting to index");
                return RedirectToAction("Index");
            }

            HttpContext.Session.TryGetValue("TotpSecretRegisterKey", out var totpSecretRegisterKey);
            if(totpSecretRegisterKey == null)
                totpSecretRegisterKey = KeyGeneration.GenerateRandomKey(16);
            

            HttpContext.Session.Set("TotpSecretRegisterKey", totpSecretRegisterKey);

            return View(totpSecretRegisterKey);
        }


        private bool CheckIfAlreadyLoggedIn()
        {
            var user = this.GetUserFromHttpContext();
            if (user != null)
            {
                Console.WriteLine("user is already logged in, redirecting...");
                return true;
            }
            return false;
        }

    }
}