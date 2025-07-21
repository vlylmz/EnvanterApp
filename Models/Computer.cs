using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Computer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bilgisayar adı zorunludur")]
    [StringLength(100, ErrorMessage = "Bilgisayar adı en fazla 100 karakter olabilir")]
    [Display(Name = "Bilgisayar Adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Varlık etiketi zorunludur")]
    [StringLength(50, ErrorMessage = "Varlık etiketi en fazla 50 karakter olabilir")]
    [Display(Name = "Varlık Etiketi")]
    public string AssetTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seri numarası zorunludur")]
    [StringLength(100, ErrorMessage = "Seri numarası en fazla 100 karakter olabilir")]
    [Display(Name = "Seri Numarası")]
    public string SerialNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Firma seçimi zorunludur")]
    [Display(Name = "Firma")]
    public int CompanyId { get; set; }

    [Display(Name = "Atanan Çalışan")]
    public int? AssignedEmployeeId { get; set; }

    // --- Enum tabanlı Status ---
    [Required]
    [Display(Name = "Durum")]
    public ComputerStatus Status { get; set; } = ComputerStatus.Havuzda;

    // Donanım özellikleri
    [StringLength(255)]
    public string? ProcessorName { get; set; }

    [Range(0.1, 10.0)]
    public decimal? ProcessorSpeed { get; set; }

    [Range(1, 128)]
    public int? ProcessorCores { get; set; }

    [Range(1, 1024)]
    public int? RamAmount { get; set; }

    [StringLength(50)]
    public string? RamType { get; set; }

    [Range(800, 8000)]
    public int? RamSpeed { get; set; }

    [Display(Name = "Depolama Tipi")]
    public StorageType? StorageType { get; set; }

    [Range(1, 10000)]
    public int? StorageSize { get; set; }

    [StringLength(255)]
    public string? GraphicsCard { get; set; }

    [StringLength(100)]
    public string? OperatingSystem { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? WarrantyEndDate { get; set; } // (WarrantyExpiry ile aynı)

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdatedDate { get; set; }
    public string? LastUpdatedBy { get; set; }

    // Hatalar yüzünden eklenen migration uyumlu property'ler:
    public string? StatusDisplayName { get; set; }
    public string? StatusBadgeClass { get; set; }

    // Navigation
    public Company? Company { get; set; }
    public Employee? AssignedEmployee { get; set; }

    // Computed Properties (opsiyonel)
    public bool IsWarrantyExpired => WarrantyEndDate.HasValue && WarrantyEndDate < DateTime.Now;

    public int? WarrantyDaysRemaining => WarrantyEndDate.HasValue
        ? Math.Max(0, (int)(WarrantyEndDate.Value - DateTime.Now).TotalDays)
        : null;
}

// --- Enumlar ---
public enum ComputerStatus
{
    Havuzda = 0,
    Zimmetli = 1,
    Kullanımda = 2,
    Bakim = 3,
    Arızalı = 4,
    Aktif = 5,
    Bozuk = 6
}

public enum StorageType
{
    HDD = 0,
    SSD = 1,
    NVMe = 2,
    Hybrid = 3,
    Diğer = 4
}
