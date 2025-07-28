using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AccountController(AppDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home"); // or wherever your login page is
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserRole = user.UserRole,
                CreatedDate = user.CreatedDate,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return View(model);
        }

        // GET: Account/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return View(model);
        }

        // POST: Account/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Check if email is already taken by another user
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.Id != user.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            // Update user properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.TwoFactorEnabled = model.TwoFactorEnabled;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu.");
                return View(model);
            }
        }

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            // Verify current password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("CurrentPassword", "Mevcut şifre yanlış.");
                return View(model);
            }

            // Hash and set new password
            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction(nameof(Profile));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Şifre değiştirme sırasında bir hata oluştu.");
                return View(model);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        // GET: Account/Settings
        public async Task<IActionResult> Settings()
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var model = new UserSettingsViewModel
            {
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailNotifications = true, // You can add these fields to ApplicationUser if needed
                SmsNotifications = false
            };

            return View(model);
        }

        // Helper method to get current user
        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var username = HttpContext.Session.GetString("user");
            if (string.IsNullOrEmpty(username))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);
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