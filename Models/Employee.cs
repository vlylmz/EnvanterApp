using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Firma seçimi zorunludur")]
        [Display(Name = "Firma")]
        public int CompanyId { get; set; }

        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTimeOffset ActiveTime { get; set; }

        public string Password { get; set; } = "Manisa.45";

        // Navigation Property
        public Company? Company { get; set; }

        public string Title { get; set; }
    }
}