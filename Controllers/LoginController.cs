using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using QRCoder;
using WebApplication1.Models;

public class LoginController : Controller
{
    public IActionResult Index()
    {
        // redirect to dashboard if already logged in
        if (HttpContext.Session.GetString("user") != null)
            return RedirectToAction("Index", "Dashboard");

        // return Login screen
        return View();
    }


    [HttpPost]
    public IActionResult Index(string username, string password)
    {
        if (HttpContext.Session.GetString("user") != null)
            return RedirectToAction("Index", "Dashboard");

        // Check Login info
        var user = getUserInfoFromDB();
        

        if (user != null)
        {
            //Console.WriteLine(user.userName + " " + user.totpSecret);
            HttpContext.Session.SetString("2FA_auth_wait_user", username);
            if(user.totpSecret != null)
                HttpContext.Session.Set("2FA_auth_secret", user.totpSecret);
            else
            {
                ViewBag.Error = "empty TOTP secret for user: " + user.userName;
                HttpContext.Session.Clear();
                return View("Index");
            }

            return View("get2FA");
        }

        else if(username == "admin" && password == "1234")
        {
            //admin first login
            HttpContext.Session.SetString("2FA_auth_add_wait_user", "admin");

            var secretKey = KeyGeneration.GenerateRandomKey(16);
            var base32Secret = Base32Encoding.ToString(secretKey);

            HttpContext.Session.Set("2FA_auth_add_new_secret", secretKey);

            string issuer = "Envanter";
            string otpauthUrl = $"otpauth://totp/{issuer}?secret={base32Secret}&digits=6";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.L);
            var pngByteQrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeByte = pngByteQrCode.GetGraphic(20);
            string base64Image = Convert.ToBase64String(qrCodeByte);

            var qrUrl = $"data:image/png;base64,{base64Image}";
            ViewBag.qrUrl = qrUrl;
            HttpContext.Session.SetString("qrUrl", qrUrl);

            return View("set2FA");
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
        return View();
    }


    private User? getUserInfoFromDB()
    {
        if(!System.IO.File.Exists("Data/userData.json"))
        {
            Console.WriteLine("recreating db");
            System.IO.File.WriteAllText("Data/userData.json", "");
            return null;
        }

        //Console.WriteLine("found db");
        var json = System.IO.File.ReadAllText("Data/userData.json");
        //Console.WriteLine("db content: " + json);
        if(!string.IsNullOrEmpty(json))
        {
            
            try
            {
                return JsonSerializer.Deserialize<User>(json);
            }
            catch(JsonException ex) { Console.WriteLine("json ex"); return null; }
            catch(Exception ex) { Console.WriteLine("Unexpected: " + ex.Message); return null; }
        }
        Console.WriteLine("json nf");
        return null;
    }


    [HttpPost]
    public IActionResult register2FA(string code_2fa)
    {
        string? user = HttpContext.Session.GetString("2FA_auth_add_wait_user");
        if (user == null || string.IsNullOrEmpty(user))
        {
            ViewBag.Error = "No user at session";
            return View("Index");
        }

        var totpSecretHttp = HttpContext.Session.Get("2FA_auth_add_new_secret");
        var totpSecretObj = new Totp(totpSecretHttp);
        var totpSecretComputed = totpSecretObj.ComputeTotp();

        if (code_2fa != totpSecretComputed)
        {
            ViewBag.Error = "Wrong 2FA key";
            ViewBag.qrUrl = HttpContext.Session.GetString("qrUrl");
            return View("set2FA");
        }

        // save user into DB
        User newUser = new User() { id = 0, realName = "Hakkı", surname = "Ayman", userName = "admin", registerDate = DateTime.UtcNow, totpSecret = totpSecretHttp };
        string userJson = JsonSerializer.Serialize<User>(newUser);
        //Console.WriteLine(userJson);
        System.IO.File.WriteAllText("Data/userData.json", userJson);

        HttpContext.Session.Clear();
        HttpContext.Session.SetString("user", user);
        return RedirectToAction("Index", "Dashboard");
    }


    [HttpPost]
    public IActionResult login2FA(string code_2fa)
    {
        string? user = HttpContext.Session.GetString("2FA_auth_wait_user");
        if (user == null || string.IsNullOrEmpty(user))
        {
            user = HttpContext.Session.GetString("2FA_auth_add_wait_user");
            if (user == null || string.IsNullOrEmpty(user))
            {
                ViewBag.Error = "No user at session";
                return View("Index");
            }
        }

        var totpSecretHttp = HttpContext.Session.Get("2FA_auth_secret");
        if(totpSecretHttp == null)
            totpSecretHttp = HttpContext.Session.Get("2FA_auth_add_new_secret");
        if(totpSecretHttp == null)
        {
            ViewBag.Error = "empty TOTP secret for user at login";
            return View("Index");
        }

        var totpSecretObj = new Totp(totpSecretHttp);
        var totpSecretComputed = totpSecretObj.ComputeTotp();

        if (code_2fa != totpSecretComputed)
        {
            ViewBag.Error = "Wrong 2FA key";
            return View("get2FA");
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetString("user", user);
        return RedirectToAction("Index", "Dashboard");
    }


    public IActionResult pass2FA()
    {
        string? user = HttpContext.Session.GetString("2FA_auth_add_wait_user");
        if (user != null && !string.IsNullOrEmpty(user))
        {
            HttpContext.Session.Clear();

            if (user != "admin")
            {
                ViewBag.Error = "non-admin users can't pass 2FA";
                return View("Index");
            }

            HttpContext.Session.SetString("user", user);
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Error = "empty user at pass2FA";
        return View("set2FA");
    }

    
    public IActionResult logOut()
    {
        HttpContext.Session.Clear();
        return View("Index");
    }

}
