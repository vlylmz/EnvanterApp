using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using QRCoder;
using WebApplication1.Models;
using static QRCoder.PayloadGenerator;
using WebApplication1;

public class LoginController : Controller
{
    public IActionResult Index()
    {
        // redirect to dashboard if already logged in
        if (EnvanterLib.getUserFromSession(this) != null)
            return RedirectToAction("Index", "Dashboard");

        // return Login screen
        return View("Index");
    }


    [HttpPost]
    public IActionResult Index(string username, string password)
    {
        if (EnvanterLib.getUserFromSession(this) != null)
            return RedirectToAction("Index", "Dashboard");

        // Check Login info
        User? user = getUserInfoFromDB();
        
        if (user != null)
        {
            if(user.TotpSecret != null)
            {
                HttpContext.Session.SetString("2FA_userToAuth", JsonSerializer.Serialize<User>(user));
                return View("get2FA");
            }
            else
            {
                HttpContext.Session.Clear();
                saveUsertoSession(user);
                return RedirectToAction("Index", "Dashboard");
            }
        }

        else if(username == "admin" && password == "1234")
        {
            // admin first login
            
            var secretKey = KeyGeneration.GenerateRandomKey(16);

            User adminAccount = new User() { Id = 0, UserName = "admin", FirstName = "Hakkı", LastName = "Ayman", Email = "asdasd@example.org", CreatedTime = DateTime.Now, TotpSecret = secretKey};
            HttpContext.Session.SetString("2FA_userToAdd", JsonSerializer.Serialize<User>(adminAccount));

            var base32Secret = Base32Encoding.ToString(secretKey);

            string issuer = "Envanter";
            string otpauthUrl = $"otpauth://totp/{issuer}?secret={base32Secret}&digits=6";

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(otpauthUrl, QRCodeGenerator.ECCLevel.L);
            var pngByteQrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeByte = pngByteQrCode.GetGraphic(20);
            string base64Image = Convert.ToBase64String(qrCodeByte);

            var qrUrl = $"data:image/png;base64,{base64Image}";
            ViewBag.qrUrl = qrUrl;
            HttpContext.Session.SetString("2FA_qrUrl", qrUrl);

            return View("set2FA");
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
        return View("Index");
    }


    private static User? getUserInfoFromDB()
    {
        if(!System.IO.File.Exists("Data/userData.json"))
        {
            System.IO.File.WriteAllText("Data/userData.json", "");
            return null;
        }

        var json = System.IO.File.ReadAllText("Data/userData.json");
        if(!string.IsNullOrEmpty(json))
        {
            
            try
            {
                return JsonSerializer.Deserialize<User>(json);
            }
            catch(JsonException ex) { Console.WriteLine("json ex" + ex.Message); return null; }
            catch(Exception ex) { Console.WriteLine("Unexpected: " + ex.Message); return null; }
        }

        return null;
    }


    [HttpPost]
    public IActionResult register2FA(string code_2fa)
    {
        var userToAddJson = HttpContext.Session.GetString("2FA_userToAdd");
        if(userToAddJson == null || string.IsNullOrEmpty(userToAddJson))
        {
            ViewBag.Error = "empty user";
            return View("set2FA");
        }

        User? userToAdd;

        try
        {
            userToAdd = JsonSerializer.Deserialize<User>(userToAddJson);
            if(userToAdd == null)
            {
                ViewBag.Error = "unknown";
                return View("set2FA");
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
            ViewBag.Error = "Error at deserialize";
            ViewBag.qrUrl = HttpContext.Session.GetString("2FA_qrUrl");
            return View("set2FA");    
        }

        var totpSecretHttp = userToAdd.TotpSecret;
        var totpSecretObj = new Totp(totpSecretHttp);
        var totpSecretComputed = totpSecretObj.ComputeTotp();

        if (code_2fa != totpSecretComputed)
        {
            ViewBag.Error = "Wrong 2FA key";
            ViewBag.qrUrl = HttpContext.Session.GetString("2FA_qrUrl");
            return View("set2FA");
        }

        // save user into DB
        saveUserToDB(userToAdd);
        
        // login
        HttpContext.Session.Clear();
        saveUsertoSession(userToAdd);
        return RedirectToAction("Index", "Dashboard");
    }


    [HttpPost]
    public IActionResult login2FA(string code_2fa)
    {
        var userToAuthJson = HttpContext.Session.GetString("2FA_userToAuth");
        if(userToAuthJson == null || string.IsNullOrEmpty(userToAuthJson))
        {
            ViewBag.Error = "empty user at login";
            return View("get2FA");
        }
        
        User? userToAuth = JsonSerializer.Deserialize<User>(userToAuthJson);
        if (userToAuth == null)
        {
            ViewBag.Error = "unknown";
            return View("get2FA");
        }

        var totpSecretObj = new Totp(userToAuth.TotpSecret);
        var totpSecretComputed = totpSecretObj.ComputeTotp();

        if (code_2fa != totpSecretComputed)
        {
            ViewBag.Error = "Wrong 2FA key";
            return View("get2FA");
        }

        HttpContext.Session.Clear();
        saveUsertoSession(userToAuth);
        return RedirectToAction("Index", "Dashboard");
    }


    public IActionResult pass2FA()
    {
        var userJson = HttpContext.Session.GetString("2FA_userToAdd");
        if(userJson != null)
        {
            User? user = JsonSerializer.Deserialize<User>(userJson);
            if (user != null && user.UserName == "admin")
            {
                HttpContext.Session.Clear();
                saveUsertoSession(user);
                return RedirectToAction("Index", "Dashboard");
            }
        }

        ViewBag.Error = "error";
        return View("set2FA");
    }


    public IActionResult logOut()
    {
        HttpContext.Session.Clear();
        return View("Index");
    }

    private void saveUsertoSession(User user)
    {
        var json = JsonSerializer.Serialize<User>(user);
        HttpContext.Session.SetString("userJson", json);
    }


    private static void saveUserToDB(User user)
    {
        System.IO.File.AppendAllText("Data/userData.json", JsonSerializer.Serialize<User>(user));
    }
}
