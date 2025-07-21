using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Employee
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Ad alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;    
    [Required(ErrorMessage = "Soyad alanı zorunludur")]
    [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Firma seçimi zorunludur")]
    [Display(Name = "Firma")]
    public int CompanyId { get; set; }
    
    [StringLength(20)]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }
    
    [StringLength(255)]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    [Display(Name = "Email")]
    public string? Email { get; set; }
    
    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
    
    [StringLength(100)]
    [Display(Name = "Departman")]
    public string? Department { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Pozisyon")]
    public string? Position { get; set; }
    
    [Display(Name = "İşe Başlama Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }
    
    [StringLength(20)]
    [Display(Name = "Personel Numarası")]
    public string? EmployeeNumber { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(255)]
    public string Password { get; set; } = "Manisa.45";
    
    // Navigation Properties
    public Company? Company { get; set; }
    public ICollection<Computer> AssignedComputers { get; set; } = new List<Computer>();
    public ICollection<Software> AssignedSoftwares { get; set; } = new List<Software>();
    public ICollection<Supply> AssignedSupplies { get; set; } = new List<Supply>();
    
    // Computed Properties
    public string FullName => $"{FirstName} {LastName}";
    
    public int TotalAssignedAssets => 
        AssignedComputers.Count + AssignedSoftwares.Count + AssignedSupplies.Sum(s => s.Quantity);
    
    public TimeSpan? WorkDuration => StartDate.HasValue 
        ? DateTime.Now - StartDate.Value 
        : null;
        
    public string WorkDurationText => WorkDuration?.TotalDays switch
    {
        null => "Belirtilmemiş",
        < 30 => "Yeni",
        < 365 => $"{(int)(WorkDuration.Value.TotalDays / 30)} ay",
        _ => $"{(int)(WorkDuration.Value.TotalDays / 365)} yıl"
    };
}

// Common departments
public static class Departments
{
    public static readonly List<string> List = new()
    {
        "Bilgi İşlem",
        "İnsan Kaynakları",
        "Muhasebe",
        "Satış",
        "Pazarlama",
        "Üretim",
        "Kalite Kontrol",
        "Lojistik",
        "Genel Müdürlük",
        "Diğer"
    };
}

// Extension methods for Employee
public static class EmployeeExtensions
{
    public static bool HasAssets(this Employee employee) => 
        employee.TotalAssignedAssets > 0;
        
    public static bool IsNewEmployee(this Employee employee, int daysThreshold = 90) =>
        employee.StartDate.HasValue && 
        (DateTime.Now - employee.StartDate.Value).TotalDays <= daysThreshold;
}