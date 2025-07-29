using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Profile
        public IActionResult Profile()
        {         
            // Check 2FA status from session - only active if secret exists
            var totpSecret = HttpContext.Session.GetString("adminTotpSecret");
            var has2FA = !string.IsNullOrEmpty(totpSecret);
            
            var model = new UserProfileViewModel
            {
                Id = "mock-admin-001",
                UserName = "admin",
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                PhoneNumber = "+90 555 123 4567",
                UserRole = "Süper Admin",
                CreatedDate = new DateTime(2024, 1, 1),
                TwoFactorEnabled = has2FA
            };

            return View(model);
        }

        // GET: Account/EditProfile
        public IActionResult EditProfile()
        {
            // Check 2FA status from session
            var totpSecret = HttpContext.Session.GetString("adminTotpSecret");
            var has2FA = !string.IsNullOrEmpty(totpSecret);
            
            var model = new EditProfileViewModel
            {
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                PhoneNumber = "+90 555 123 4567",
                TwoFactorEnabled = has2FA
            };

            return View(model);
        }

        // POST: Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Handle 2FA toggle - can only be disabled, not re-enabled
            var currentTotpSecret = HttpContext.Session.GetString("adminTotpSecret");
            var current2FA = !string.IsNullOrEmpty(currentTotpSecret);
            
            if (!model.TwoFactorEnabled && current2FA)
            {
                // Disable 2FA - permanent action
                HttpContext.Session.Remove("adminTotpSecret");
                TempData["Success"] = "İki faktörlü doğrulama kalıcı olarak devre dışı bırakıldı.";
            }
            else if (model.TwoFactorEnabled && !current2FA)
            {
                // Cannot re-enable 2FA once disabled
                TempData["Error"] = "İki faktörlü doğrulama bir kez devre dışı bırakıldıktan sonra tekrar etkinleştirilemez.";
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
            }

            return RedirectToAction(nameof(Profile));
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Keys.Where(key => key != "adminPassword" && key != "adminTotpSecret").ToList().ForEach(key => HttpContext.Session.Remove(key));
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Settings
        public IActionResult Settings()
        {
            // Check 2FA status from session
            var totpSecret = HttpContext.Session.GetString("adminTotpSecret");
            var has2FA = !string.IsNullOrEmpty(totpSecret);
            
            var model = new UserSettingsViewModel
            {
                TwoFactorEnabled = has2FA,
                EmailNotifications = true,
                SmsNotifications = false
            };

            return View(model);
        }

        // POST: Account/Disable2FA - AJAX endpoint for disabling 2FA (one-way only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Disable2FA()
        {
            var current2FA = HttpContext.Session.GetString("adminTotpSecret");
            
            if (!string.IsNullOrEmpty(current2FA))
            {
                // Disable 2FA - one way only
                HttpContext.Session.Remove("adminTotpSecret");
                HttpContext.Session.SetString("2FADisabled", "true");
                return Json(new { success = true, message = "İki faktörlü doğrulama kalıcı olarak devre dışı bırakıldı." });
            }
            else
            {
                return Json(new { success = false, message = "İki faktörlü doğrulama zaten devre dışı." });
            }
        }

        // POST: Account/QuickPasswordChange - AJAX endpoint for password change
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuickPasswordChange(string currentPassword, string newPassword)
        {
            // Get current password from session or use default
            var storedPassword = HttpContext.Session.GetString("adminPassword") ?? "admin123";

            if (currentPassword != storedPassword)
            {
                return Json(new { success = false, message = "Mevcut şifre yanlış." });
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Yeni şifre en az 6 karakter olmalıdır." });
            }

            // Save new password to session
            HttpContext.Session.SetString("adminPassword", newPassword);

            return Json(new { success = true, message = "Şifreniz başarıyla değiştirildi." });
        }
    }

    // ViewModels
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [MaxLength(50)]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [MaxLength(50)]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100)]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }

        [Display(Name = "İki Faktörlü Doğrulama")]
        public bool TwoFactorEnabled { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve onay şifresi eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserSettingsViewModel
    {
        [Display(Name = "İki Faktörlü Doğrulama")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "E-posta Bildirimleri")]
        public bool EmailNotifications { get; set; }

        [Display(Name = "SMS Bildirimleri")]
        public bool SmsNotifications { get; set; }
    }
}