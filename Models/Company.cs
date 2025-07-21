using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur")]
        [MaxLength(100)]
        [Display(Name = "Şirket Adı")]
        public string Name { get; set; }

        [MaxLength(255)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        [MaxLength(100)]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [MaxLength(20)]
        [Display(Name = "Telefon")]
        public string Phone { get; set; }

        [MaxLength(200)]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [MaxLength(50)]
        [Display(Name = "Şehir")]
        public string City { get; set; }

        [MaxLength(50)]
        [Display(Name = "İlçe")]
        public string District { get; set; }

        [MaxLength(10)]
        [Display(Name = "Posta Kodu")]
        public string PostalCode { get; set; }

        [MaxLength(50)]
        [Display(Name = "Ülke")]
        public string Country { get; set; } = "Türkiye";

        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        [MaxLength(200)]
        [Display(Name = "Web Sitesi")]
        public string Website { get; set; }

        [MaxLength(100)]
        [Display(Name = "Logo URL")]
        public string LogoUrl { get; set; }

        // Çalışan sayısı
        [Display(Name = "Çalışan Sayısı")]
        [Range(0, int.MaxValue, ErrorMessage = "Çalışan sayısı 0'dan büyük olmalıdır")]
        public int? EmployeeCount { get; set; }

        // Durumlar
        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Onaylı Mı?")]
        public bool IsVerified { get; set; } = false;

        // Notlar
        [MaxLength(1000)]
        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)] 
        public string Notes { get; set; }

        // Denetim alanları
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedDate { get; set; }

        [MaxLength(100)]
        [Display(Name = "Oluşturan")]
        public string CreatedBy { get; set; }

        [MaxLength(100)]
        [Display(Name = "Güncelleyen")]
        public string UpdatedBy { get; set; }

        // İletişim kişisi bilgileri
        [MaxLength(100)]
        [Display(Name = "Yetkili Kişi")]
        public string ContactPerson { get; set; }

        [MaxLength(50)]
        [Display(Name = "Yetkili Unvanı")]
        public string ContactTitle { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        [Display(Name = "Yetkili E-posta")]
        public string ContactEmail { get; set; }

        [Phone]
        [MaxLength(20)]
        [Display(Name = "Yetkili Telefon")]
        public string ContactPhone { get; set; }

        // İlişkisel veriler için navigation properties
        // public virtual ICollection<Employee> Employees { get; set; }
        // public virtual ICollection<Product> Products { get; set; }
        // public virtual ICollection<Order> Orders { get; set; }
    }
}