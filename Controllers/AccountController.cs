using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        
        if (username == "admin" && password == "1234")
        {
            // Giriş başarılıysa Home sayfasına yönlendir
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
        return View();
    }
}
