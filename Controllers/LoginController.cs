using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using WebApplication1.Models;
using static WebApplication1.EnvanterLib;


namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        readonly string Pending_2fa_user_add_json = "Pending_2fa_user_add_json";
        readonly string Pending_2fa_user_login_json = "Pending_2fa_user_login_json";
        readonly string Pending_2fa_secret_key = "Pending_2fa_secret_key";



        public IActionResult Index()
        {
            // redirect to dashboard if already logged in
            if (GetUserFromSession(this) != null)
                return RedirectToAction("Index", "Dashboard");

            // return Login screen
            return View();
        }


        [NonAction]
        private IActionResult Index(string errorMessage)
        {
            ViewBag.Error = errorMessage;
            return View();
        }

        
        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            if (GetUserFromSession(this) != null)
                return RedirectToAction("Index", "Dashboard");

            // Check Login info

            User? returned = GetUserInfoFromDB(username, password);

            // Valid user found
            if (returned != null)
            {

                if (returned.TotpSecret != null)
                {
                    // has 2FA
                    SaveToHttpSession(Pending_2fa_user_login_json, JsonSerializer.Serialize<User>(returned));
                    return View("get2FA");
                }
                else
                {
                    HttpContext.Session.Clear();
                    RegisterUserSession(returned, clearSession: true);
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            // admin first login
            else if (username == "admin" && password == "1234")
            {

                // register page or ask password
                var adminNewPassword = "1234";

                byte[] salt = RandomNumberGenerator.GetBytes(16);

                var pbkdf2 = new Rfc2898DeriveBytes(adminNewPassword, salt, 100_000, HashAlgorithmName.SHA256);

                byte[] hash = pbkdf2.GetBytes(32);

                string storedHash = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

                User adminAccount = new()
                {
                    Id = 0,
                    UserName = "admin",
                    FirstName = "Hakkı",
                    LastName = "Ayman",
                    Email = "asdasd@example.org",
                    CreatedTime = DateTime.Now
                    ,
                    PasswordHash = storedHash
                };

                SaveUserToDB(adminAccount);
                SaveToHttpSession(Pending_2fa_user_add_json, JsonSerializer.Serialize<User>(adminAccount));
                SaveToHttpSession(Pending_2fa_secret_key, Convert.ToBase64String(KeyGeneration.GenerateRandomKey(16)));

                return View("set2FA", adminAccount);
            }

            else
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                return View("Index");
            }
        }


        [HttpPost]
        public IActionResult Register2FA(string code_2fa)
        {
            var userJson = GetFromHttpSession(Pending_2fa_user_add_json);
            var secret = GetFromHttpSession(Pending_2fa_secret_key);
            if (userJson == null || secret == null)
            {
                ViewBag.Error = "error at registering2FA (empty pending user or key)";

                return View("Index");
            }

            User? user = JsonSerializer.Deserialize<User>(userJson);

            if (user == null)
            {
                ViewBag.Error = "???";
                return View("Index");
            }
            user.TotpSecret = Convert.FromBase64String(secret);
            var totpObj = new Totp(user.TotpSecret);
            var totpComputed = totpObj.ComputeTotp();

            if (code_2fa != totpComputed)
            {
                ViewBag.Error = "Wrong 2FA key";
                return View("set2FA", user);
            }

            // add 2FA to user
            UpdateUserInfo(user);

            // login
            RegisterUserSession(user, clearSession: true);
            return RedirectToAction("Index", "Dashboard");
        }


        [HttpPost]
        public IActionResult Login2FA(string code_2fa)
        {
            var userJson = GetFromHttpSession(Pending_2fa_user_login_json);
            if (userJson == null)
            {
                ViewBag.Error = "Empty pending 2fa user";
                return View("get2FA");
            }

            User? user = JsonSerializer.Deserialize<User>(userJson);
            if (user == null)
            {
                ViewBag.Error = "???";
                return View("get2FA");
            }


            var userTotp = new Totp(user.TotpSecret);
            var userTotpComputed = userTotp.ComputeTotp();

            if (code_2fa != userTotpComputed)
            {
                ViewBag.Error = "Wrong 2FA key";
                return View("get2FA");
            }

            RegisterUserSession(user, clearSession: true);
            return RedirectToAction("Index", "Dashboard");
        }


        public IActionResult Pass2FA()
        {
            // currently only implemented for admin

            var userJson = GetFromHttpSession(Pending_2fa_user_add_json);
            if (userJson != null)
            {
                var user = JsonSerializer.Deserialize<User>(userJson);
                if (user != null && user.UserName == "admin")
                {
                    RegisterUserSession(user, clearSession: true);
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            ViewBag.Error = "you're not admin";
            return View("set2FA");
        }


        [NonAction]
        private static User? GetUserInfoFromDB(string username, string password)
        {
            if (!System.IO.File.Exists("Data/userData.json"))
            {
                System.IO.File.WriteAllText("Data/userData.json", "");
                return null;
            }

            var json = System.IO.File.ReadAllText("Data/userData.json");
            if (!string.IsNullOrEmpty(json))
            {

                try
                {
                    return JsonSerializer.Deserialize<User>(json);
                }
                catch (JsonException ex) { Console.WriteLine("json ex" + ex.Message); return null; }
                catch (Exception ex) { Console.WriteLine("Unexpected: " + ex.Message); return null; }
            }

            return null;
        }


        [NonAction]
        private void RegisterUserSession(User user, bool clearSession = false)
        {
            if (clearSession)
                HttpContext.Session.Clear();
            var json = JsonSerializer.Serialize<User>(user);
            HttpContext.Session.SetString("userJson", json);
            //HttpAuthorize(user, false);
        }

        [NonAction]
        private static void SaveUserToDB(User user)
        {
            System.IO.File.WriteAllText("Data/userData.json", JsonSerializer.Serialize<User>(user));
        }

        [NonAction]
        private static void UpdateUserInfo(User user)
        {
            // şimdilik
            SaveUserToDB(user);
        }

        [NonAction]
        private void SaveToHttpSession(string str, string data)
        {
            HttpContext.Session.SetString(str, data);
        }

        [NonAction]
        private string? GetFromHttpSession(string str)
        {
            return HttpContext.Session.GetString(str);
        }


        /*[NonAction]
        private void HttpAuthorize(User user, bool isTotp)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Totp", (isTotp)? "1" : "0")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties).GetAwaiter().GetResult();
        }*/
    }
}
