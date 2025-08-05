using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Services;

namespace WebApplication1.Models
{
    public class Software : IHasLogs
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Marka")]
        public string? Brand { get; set; }

        [Required]
        [Display(Name = "Satın Alma Tarihi")]
        public DateTime PurchaseDate { get; set; }

        [Required]
        [Display(Name = "Süre")]
        public string? Duration { get; set; }

        [Required]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime CompletionDate { get; set; }

        [Required]
        [Display(Name = "Son Kullanma Tarihi")]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Kalan Süre")]
        public string? RemainingTime { get; set; }

        [Display(Name = "Durum")]
        public SoftwareStatus Status { get; set; }

        [Display(Name = "Görsel Uyarı")]
        public AlertColor AlertColor { get; set; }

        [Display(Name = "Atanmış Çalışan")]
        public int? AssignedEmployeeId { get; set; }

        [ForeignKey("AssignedEmployeeId")]
        [Display(Name = "Atanmış Çalışan")]
        public virtual Employee? AssignedEmployee { get; set; }

        [Display(Name = "Firma")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        [Display(Name = "Firma")]
        public virtual Company? Company { get; set; }

        [Display(Name = "Lisans Anahtarı")]
        public string? LicenseKey { get; set; }

        [Display(Name = "Seri Numarası")]
        public string? SerialNumber { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Güncelleme Tarihi")]
        public DateTime? UpdatedDate { get; set; }
        
        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; } = true;

        
    }
    
    public enum SoftwareStatus
    {
        [Display(Name = "Aktif")]
        Active,
        
        [Display(Name = "Süresi Dolmuş")]
        Expired,
        
        [Display(Name = "Yaklaşan")]
        Approaching
    }
    
    public enum AlertColor
    {
        [Display(Name = "Kırmızı")]
        Red,
        
        [Display(Name = "Sarı")]
        Yellow,
        
        [Display(Name = "Yeşil")]
        Green
    }
}