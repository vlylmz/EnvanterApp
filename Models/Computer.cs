using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Computer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bilgisayar adı zorunludur")]
        [StringLength(100, ErrorMessage = "Bilgisayar adı en fazla 100 karakter olabilir")]
        [Display(Name = "Bilgisayar Adı")]
        public string? Name { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Display(Name = "Asset Tag")]
        [StringLength(50, ErrorMessage = "Asset tag en fazla 50 karakter olabilir")]
        public string? AssetTag { get; set; }

        [Display(Name = "Seri Numarası")]
        [StringLength(100, ErrorMessage = "Seri numarası en fazla 100 karakter olabilir")]
        public string? SerialNumber { get; set; }

        [Display(Name = "Marka")]
        [StringLength(50, ErrorMessage = "Marka en fazla 50 karakter olabilir")]
        public string? Brand { get; set; } = "Dell";

        [Display(Name = "Model")]
        [StringLength(100, ErrorMessage = "Model en fazla 100 karakter olabilir")]
        public string? Model { get; set; }

        [Display(Name = "İşlemci")]
        [StringLength(100, ErrorMessage = "İşlemci bilgisi en fazla 100 karakter olabilir")]
        public string? Processor { get; set; }

        [Display(Name = "RAM (GB)")]
        [Range(1, 1024, ErrorMessage = "RAM 1-1024 GB arasında olmalıdır")]
        public int? RamGB { get; set; }

        [Display(Name = "Depolama (GB)")]
        [Range(1, 10240, ErrorMessage = "Depolama 1-10240 GB arasında olmalıdır")]
        public int? StorageGB { get; set; }

        [Display(Name = "Depolama Türü")]
        public StorageType? StorageType { get; set; }

        [Display(Name = "İşletim Sistemi")]
        [StringLength(100, ErrorMessage = "İşletim sistemi bilgisi en fazla 100 karakter olabilir")]
        public string? OperatingSystem { get; set; }

        [Required(ErrorMessage = "Şirket ID zorunludur")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Display(Name = "Atanan Çalışan")]
        public int? AssignedEmployeeId { get; set; }

        [Display(Name = "Durum")]
        public ComputerStatus Status { get; set; } = ComputerStatus.Aktif;

        [Display(Name = "Konum")]
        [StringLength(200, ErrorMessage = "Konum bilgisi en fazla 200 karakter olabilir")]
        public string? Location { get; set; }

        [Display(Name = "Satın Alma Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "Garanti Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? WarrantyEndDate { get; set; }

        [Display(Name = "Satın Alma Fiyatı")]
        [Range(0, 999999, ErrorMessage = "Fiyat 0-999999 arasında olmalıdır")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchasePrice { get; set; }

        [Display(Name = "IP Adresi")]
        [StringLength(15, ErrorMessage = "IP adresi en fazla 15 karakter olabilir")]
        public string? IpAddress { get; set; }

        [Display(Name = "MAC Adresi")]
        [StringLength(17, ErrorMessage = "MAC adresi en fazla 17 karakter olabilir")]
        public string? MacAddress { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Son Güncelleme Tarihi")]
        public DateTime? LastUpdatedDate { get; set; }

        [Display(Name = "Güncelleyen Kullanıcı")]
        [StringLength(100, ErrorMessage = "Kullanıcı adı en fazla 100 karakter olabilir")]
        public string? LastUpdatedBy { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir")]
        public string? Notes { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        // Computed Properties
        [NotMapped]
        public bool IsUnderWarranty => WarrantyEndDate.HasValue && WarrantyEndDate.Value > DateTime.Now;

        [NotMapped]
        public int? AgeInDays => PurchaseDate.HasValue ? (DateTime.Now - PurchaseDate.Value).Days : null;

        [NotMapped]
        public string StatusDisplayName => Status switch
        {
            ComputerStatus.Aktif => "Aktif",
            ComputerStatus.Pasif => "Pasif",
            ComputerStatus.Bakim => "Bakımda",
            ComputerStatus.Bozuk => "Bozuk",
            ComputerStatus.Emekliye_Ayrildi => "Emekliye Ayrıldı",
            _ => "Bilinmiyor"
        };

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            ComputerStatus.Aktif => "badge bg-success",
            ComputerStatus.Pasif => "badge bg-secondary",
            ComputerStatus.Bakim => "badge bg-warning",
            ComputerStatus.Bozuk => "badge bg-danger",
            ComputerStatus.Emekliye_Ayrildi => "badge bg-dark",
            _ => "badge bg-light text-dark"
        };
    }

    public enum ComputerStatus
    {
        [Display(Name = "Aktif")]
        Aktif = 1,
        [Display(Name = "Pasif")]
        Pasif = 2,
        [Display(Name = "Bakımda")]
        Bakim = 3,
        [Display(Name = "Bozuk")]
        Bozuk = 4,
        [Display(Name = "Emekliye Ayrıldı")]
        Emekliye_Ayrildi = 5
    }

    public enum StorageType
    {
        [Display(Name = "HDD")]
        HDD = 1,
        [Display(Name = "SSD")]
        SSD = 2,
        [Display(Name = "NVMe")]
        NVMe = 3,
        [Display(Name = "Hybrid")]
        Hybrid = 4
    }
}