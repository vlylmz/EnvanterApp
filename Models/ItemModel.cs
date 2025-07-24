using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class ItemModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [MaxLength(200)]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Kategori zorunludur")]
        [MaxLength(100)]
        [Display(Name = "Kategori")]
        public string Category { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Sistem Barkodu")]
        public string SystemBarcode { get; set; }

        // Takip bilgileri
        [MaxLength(50)]
        [Display(Name = "Zimmet Durumu")]
        public string? AssignmentStatus { get; set; } = "Unassigned"; // "Assigned", "Unassigned", "Under Maintenance"

        [MaxLength(200)]
        [Display(Name = "Konum")]
        public string Location { get; set; }

        // Zimmet bilgileri
        [MaxLength(100)]
        [Display(Name = "Zimmetli Personel")]
        public string? AssignedPersonnel { get; set; }

        [Display(Name = "Zimmet Tarihi")]
        public DateTime? AssignmentDate { get; set; }

        // Stok bilgileri
        [Display(Name = "Stok Miktarı")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0'dan büyük olmalıdır")]
        public int StockQuantity { get; set; }

        [Display(Name = "Minimum Stok Seviyesi")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stok seviyesi 0'dan büyük olmalıdır")]
        public int MinimumStockLevel { get; set; }

        // Durumlar
        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Kritik Seviyede Mi?")]
        public bool IsCriticalLevel { get; set; } = false;

        // Fiyat bilgileri
        [Display(Name = "Birim Fiyatı")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Para Birimi")]
        [MaxLength(10)]
        public string Currency { get; set; } = "TRY";

        // Tedarikçi bilgileri
        [MaxLength(100)]
        [Display(Name = "Tedarikçi")]
        public string Supplier { get; set; }

        [MaxLength(50)]
        [Display(Name = "Tedarikçi Kodu")]
        public string SupplierCode { get; set; }

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
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        [Display(Name = "Güncelleyen")]
        public string? UpdatedBy { get; set; }


        public void AssignToPersonnel(string personnel, string assignedBy = null)
        {
            AssignedPersonnel = personnel;
            AssignmentDate = DateTime.Now;
            AssignmentStatus = "Assigned";
            UpdatedDate = DateTime.Now;
            UpdatedBy = assignedBy;
        }

        public void ReturnFromAssignment(string returnedBy = null)
        {
            AssignedPersonnel = null;
            AssignmentDate = null;
            AssignmentStatus = "Unassigned";
            UpdatedDate = DateTime.Now;
            UpdatedBy = returnedBy;
        }

        public void UpdateLocation(string newLocation, string updatedBy = null)
        {
            Location = newLocation;
            UpdatedDate = DateTime.Now;
            UpdatedBy = updatedBy;
        }

        public void CheckCriticalLevel()
        {
            IsCriticalLevel = StockQuantity <= MinimumStockLevel;
        }

        // İlişkisel veriler için navigation properties
        // public virtual ICollection<ItemHistory> History { get; set; }
        // public virtual ICollection<ItemMovement> Movements { get; set; }
        // public virtual ICollection<ItemMaintenance> MaintenanceRecords { get; set; }
    }
}