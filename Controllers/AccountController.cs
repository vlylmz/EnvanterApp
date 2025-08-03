using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Data;
using WebApplication1.EnvanterLib;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Account/Profile
        public IActionResult Profile()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new UserProfileViewModel
            {
                Id = user.Id.ToString(),
                UserName = user.Email!, // Using email as username
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber > 0 ? $"+90 {user.PhoneNumber}" : "",
                UserRole = user.UserRole?.ToString() ?? "User",
                CreatedDate = user.CreatedDate,
                TwoFactorEnabled = user.TotpSecret != null && user.TotpSecret.Length > 0
            };

            return View(model);
        }

        // GET: Account/EditProfile
        public IActionResult EditProfile()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber > 0 ? user.PhoneNumber.ToString() : "",
                TotpEnabled = user.TotpSecret != null && user.TotpSecret.Length > 0
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

            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Update user properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            
            // Parse phone number
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                // Remove all non-numeric characters
                var cleanPhone = new string(model.PhoneNumber.Where(char.IsDigit).ToArray());
                if (long.TryParse(cleanPhone, out long phoneNumber))
                {
                    user.PhoneNumber = phoneNumber;
                }
            }

            // Save to database
            _context.ApplicationUsers.Update(user);
            _context.SaveChanges();

            // Update session
            this.SaveUserToHttpContext(user);

            TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction(nameof(Profile));
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Settings
        public IActionResult Settings()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var model = new UserSettingsViewModel
            {
                TwoFactorEnabled = user.TotpSecret != null && user.TotpSecret.Length > 0,
                EmailNotifications = true,
                SmsNotifications = false
            };

            return View(model);
        }

        // POST: Account/Toggle2FA - Toggle 2FA on/off
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle2FA()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Check current 2FA status
            bool has2FA = user.TotpSecret != null && user.TotpSecret.Length > 0;

            if (has2FA)
            {
                // Disable 2FA
                user.TotpSecret = null;
                
                // Save to database
                _context.ApplicationUsers.Update(user);
                _context.SaveChanges();
                
                // Update session
                this.SaveUserToHttpContext(user);
                
                TempData["Success"] = "İki faktörlü doğrulama devre dışı bırakıldı.";
                return RedirectToAction(nameof(Profile));
            }
            else
            {
                // Redirect to 2FA registration
                return RedirectToAction("2FA_Register", "Home");
            }
        }

        // POST: Account/Toggle2FAAsync - AJAX endpoint for toggling 2FA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle2FAAsync()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return Json(new { success = false, message = "Oturum bulunamadı." });
            }

            // Check current 2FA status
            bool has2FA = user.TotpSecret != null && user.TotpSecret.Length > 0;

            if (has2FA)
            {
                // Disable 2FA
                user.TotpSecret = null;
                
                // Save to database
                _context.ApplicationUsers.Update(user);
                _context.SaveChanges();
                
                // Update session
                this.SaveUserToHttpContext(user);
                
                return Json(new { 
                    success = true, 
                    message = "İki faktörlü doğrulama devre dışı bırakıldı.",
                    twoFactorEnabled = false 
                });
            }
            else
            {
                // Return redirect URL for 2FA registration
                return Json(new { 
                    success = true, 
                    redirect = Url.Action("2FA_Register", "Home"),
                    message = "2FA kayıt sayfasına yönlendiriliyorsunuz..."
                });
            }
        }

        // POST: Account/Disable2FA - AJAX endpoint for disabling 2FA (one-way only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Disable2FA()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return Json(new { success = false, message = "Oturum bulunamadı." });
            }
            
            if (user.TotpSecret != null && user.TotpSecret.Length > 0)
            {
                // Disable 2FA - one way only
                user.TotpSecret = null;
                
                // Save to database
                _context.ApplicationUsers.Update(user);
                _context.SaveChanges();
                
                // Update session
                this.SaveUserToHttpContext(user);
                
                return Json(new { success = true, message = "İki faktörlü doğrulama kalıcı olarak devre dışı bırakıldı." });
            }
            else
            {
                return Json(new { success = false, message = "İki faktörlü doğrulama zaten devre dışı." });
            }
        }

        // GET: Account/Check2FAStatus - AJAX endpoint to check 2FA status
        [HttpGet]
        public IActionResult Check2FAStatus()
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return Json(new { success = false, message = "Oturum bulunamadı." });
            }

            bool has2FA = user.TotpSecret != null && user.TotpSecret.Length > 0;
            
            return Json(new { 
                success = true, 
                twoFactorEnabled = has2FA 
            });
        }

        // POST: Account/QuickPasswordChange - AJAX endpoint for password change
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuickPasswordChange(string currentPassword, string newPassword)
        {
            var user = this.GetUserFromHttpContext();
            if (user == null)
            {
                return Json(new { success = false, message = "Oturum bulunamadı." });
            }

            if (currentPassword != user.Password)
            {
                return Json(new { success = false, message = "Mevcut şifre yanlış." });
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Yeni şifre en az 6 karakter olmalıdır." });
            }

            // Update password
            user.Password = newPassword;
            
            // Save to database
            _context.ApplicationUsers.Update(user);
            _context.SaveChanges();
            
            // Update session
            this.SaveUserToHttpContext(user);

            return Json(new { success = true, message = "Şifreniz başarıyla değiştirildi." });
        }
    }

    // ViewModels remain the same
    public class UserProfileViewModel
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserRole { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [MaxLength(50)]
        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [MaxLength(50)]
        [Display(Name = "Soyad")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [MaxLength(100)]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Display(Name = "Telefon Numarası")]
        public string? PhoneNumber { get; set; }

        public bool TotpEnabled { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifre ve onay şifresi eşleşmiyor.")]
        public string? ConfirmPassword { get; set; }
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