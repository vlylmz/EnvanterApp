using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Supply
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Sarf malzeme adı zorunludur")]
    [StringLength(200, ErrorMessage = "Sarf malzeme adı en fazla 200 karakter olabilir")]
    [Display(Name = "Sarf Malzeme Adı")]
    public required string Name { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Sistem Barkodu")]
    public string? SystemBarcode { get; set; }
    
    [Required(ErrorMessage = "Firma seçimi zorunludur")]
    [Display(Name = "Firma")]
    public required int CompanyId { get; set; }
    
    [Display(Name = "Atanan Çalışan")]
    public int? AssignedEmployeeId { get; set; }
    
    [Required]
    [StringLength(50)]
    [Display(Name = "Durum")]
    public required string Status { get; set; } = "Havuzda";
    
    [StringLength(100)]
    [Display(Name = "Kategori")]
    public string? Category { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Marka")]
    public string? Brand { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Model")]
    public string? Model { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Seri Numarası")]
    public string? SerialNumber { get; set; }
    
    [Display(Name = "Miktar")]
    [Range(1, 10000, ErrorMessage = "Miktar 1 ile 10000 arasında olmalıdır")]
    public int Quantity { get; set; } = 1;
    
    [Display(Name = "Birim Fiyat")]
    [Range(0, 1000000, ErrorMessage = "Birim fiyat 0 ile 1,000,000 arasında olmalıdır")]
    public decimal? UnitPrice { get; set; }
    
    [Display(Name = "Satın Alma Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }
    
    [Display(Name = "Garanti Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? WarrantyExpiry { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Tedarikçi")]
    public string? Supplier { get; set; }
    
    [StringLength(1000)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public Company? Company { get; set; }
    public Employee? AssignedEmployee { get; set; }
    
    // Computed Properties
    public decimal TotalPrice => (UnitPrice ?? 0) * Quantity;
    
    public bool IsWarrantyExpired => WarrantyExpiry.HasValue && WarrantyExpiry < DateTime.Now;
    
    public int? WarrantyDaysRemaining => WarrantyExpiry.HasValue 
        ? Math.Max(0, (int)(WarrantyExpiry.Value - DateTime.Now).TotalDays) 
        : null;
        
    public SupplyStatus StatusEnum => Status switch
    {
        "Havuzda" => SupplyStatus.InPool,
        "Zimmetli" => SupplyStatus.Assigned,
        "Kullanımda" => SupplyStatus.InUse,
        "Bakımda" => SupplyStatus.Maintenance,
        "Arızalı" => SupplyStatus.Broken,
        "Kayıp" => SupplyStatus.Lost,
        "İmha Edildi" => SupplyStatus.Disposed,
        _ => SupplyStatus.Unknown
    };
    
public string GetWarrantyStatus()
{
    if (WarrantyExpiry == null)
        return "Belirtilmemiş";
    var days = (WarrantyExpiry.Value - DateTime.Now).TotalDays;
    if (days < 0)
        return "Süresi Dolmuş";
    else if (days <= 30)
        return "Yakında Dolacak";
    else
        return "Geçerli";
}

}

// Supply Status Enum
public enum SupplyStatus
{
    InPool,
    Assigned,
    InUse,
    Maintenance,
    Broken,
    Lost,
    Disposed,
    Unknown
}

// Supply Categories
public static class SupplyCategories
{
    public static readonly List<string> Categories = new()
    {
        "Bilgisayar Aksesuarları", // Klavye, Mouse, Kulaklık
        "Ağ Ekipmanları", // Switch, Router, Kablo
        "Depolama", // USB, Harici Disk, SD Kart
        "Yazıcı Malzemeleri", // Toner, Kağıt, Kartuş
        "Kablolar", // HDMI, USB, Ethernet
        "Güvenlik", // Kamera, Kartlı Geçiş
        "Temizlik", // Temizlik Malzemeleri
        "Kırtasiye", // Kalem, Defter, Dosya
        "Elektrik", // Priz, Kablo, UPS
        "Mobilya", // Masa, Sandalye, Dolap
        "Diğer"
    };
}

// Common supply items with default properties
public static class CommonSupplyItems
{
    public static readonly Dictionary<string, (string Category, string? Brand)> Items = new()
    {
        { "Klavye", ("Bilgisayar Aksesuarları", "Logitech") },
        { "Mouse", ("Bilgisayar Aksesuarları", "Logitech") },
        { "Kulaklık", ("Bilgisayar Aksesuarları", "Sony") },
        { "USB Bellek", ("Depolama", "SanDisk") },
        { "HDMI Kablo", ("Kablolar", null) },
        { "Ethernet Kablo", ("Kablolar", null) },
        { "Toner", ("Yazıcı Malzemeleri", "HP") },
        { "A4 Kağıt", ("Yazıcı Malzemeleri", null) },
        { "Webcam", ("Bilgisayar Aksesuarları", "Logitech") },
        { "USB Hub", ("Bilgisayar Aksesuarları", null) }
    };
}

// Extension methods for Supply
public static class SupplyExtensions
{
    public static bool IsExpensive(this Supply supply, decimal threshold = 1000) =>
        supply.TotalPrice > threshold;
        
    public static string GetAgeDescription(this Supply supply)
    {
        if (!supply.PurchaseDate.HasValue) return "Bilinmiyor";
        
        var age = DateTime.Now - supply.PurchaseDate.Value;
        return age.TotalDays switch
        {
            < 30 => "Yeni",
            < 365 => $"{(int)(age.TotalDays / 30)} ay",
            _ => $"{(int)(age.TotalDays / 365)} yıl"
        };
    }
    
    public static bool NeedsReplacement(this Supply supply, int monthsThreshold = 24) =>
        supply.PurchaseDate.HasValue && 
        (DateTime.Now - supply.PurchaseDate.Value).TotalDays > (monthsThreshold * 30);
}

// Record for Supply Summary
public record SupplySummary(
    string Name,
    string Category,
    int Quantity,
    decimal TotalValue,
    string Status
);

// Factory for creating Supply instances
public static class SupplyFactory
{
    public static Supply CreateBasicSupply(string name, int companyId, string category = "Diğer") => new()
    {
        Name = name,
        CompanyId = companyId,
        Category = category,
        Status = "Havuzda",
        Quantity = 1,
        CreatedDate = DateTime.UtcNow
    };
    
    public static Supply CreateWithBarcode(string name, int companyId, string category = "Diğer") => new()
    {
        Name = name,
        CompanyId = companyId,
        Category = category,
        Status = "Havuzda",
        Quantity = 1,
        SystemBarcode = GenerateBarcode(),
        CreatedDate = DateTime.UtcNow
    };
    
    private static string GenerateBarcode() => 
        $"SUP-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
}