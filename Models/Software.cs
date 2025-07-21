using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Software
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Yazılım Adı")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Marka")]
        public string Brand { get; set; }

        [Required]
        [Display(Name = "Firma")]
        public int CompanyId { get; set; }

        [Display(Name = "Atanan Çalışan")]
        public int? AssignedEmployeeId { get; set; }

        [Display(Name = "Satın Alma Tarihi")]
        public DateTime? PurchaseDate { get; set; }  // Nullable DateTime

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? ExpiryDate { get; set; }    // Nullable DateTime

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Havuzda";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Company? Company { get; set; }
        public Employee? AssignedEmployee { get; set; }
    }
}