namespace WebApplication1.Models
{
    public class InventoryCreateViewModel
    {
        public string? Type { get; set; } // "Computer", "Software", "Supply"
        public string? Name { get; set; }
        public int CompanyId { get; set; }
        public string? AssetTag { get; set; }
        public string? SerialNumber { get; set; }
        public string? Description { get; set; }
        public DateTime? PurchaseDate { get; set; }

        // Bilgisayar'a özel
        public string? Cpu { get; set; }
        public int? RamGb { get; set; }

        // Yazılım'a özel
        public string? Brand { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Sarf Malzeme'ye özel
        public string? SystemBarcode { get; set; }
        public string? Category { get; set; }
    }
}