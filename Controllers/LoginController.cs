using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using QRCoder;
using WebApplication1.Models;

public class LoginController : Controller
{
    [HttpGet]
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
            HttpContext.Session.SetString("2FA_auth_wait_user", username);
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
            //string otpauthUrl = $"otpauth://totp/{issuer}:{userEmail}?secret={base32Secret}&issuer={issuer}&digits=6";
            string otpauthUrl = $"otpauth://totp/{issuer}?secret={base32Secret}&digits=6";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.L);
            var pngByteQrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeByte = pngByteQrCode.GetGraphic(20);
            string base64Image = Convert.ToBase64String(qrCodeByte);

            ViewBag.qrUrl = $"data:image/png;base64,{base64Image}";

            return View("set2FA");
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
        return View();
    }


    private User? getUserInfoFromDB()
    {
        if(!System.IO.File.Exists("/Data/userData.json"))
        {
            System.IO.File.WriteAllText("Data/userData.json", "");
            return null;
        }

        var json = System.IO.File.ReadAllText("Data/userData.json");
        if(json != null)
            return JsonSerializer.Deserialize<User>(json);

        return null;
    }


    [HttpPost]
    public IActionResult register2FA(string code)
    {
        Console.WriteLine(code);
        string? user = HttpContext.Session.GetString("2FA_auth_add_wait_user");
        if(user != null)
            HttpContext.Session.SetString("user", user);
        return RedirectToAction("Index", "Dashboard");
    }

    
    public IActionResult logOut()
    {
        HttpContext.Session.Clear();
        return View("Index");
    }

}
