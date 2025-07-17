using System;

namespace WebApplication1.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssetTag { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int CompanyId { get; set; } // Eksik olan CompanyId özelliği eklendi
        public string SerialNumber { get; set; } // Eksik olan SerialNumber özelliği eklendi
        public int? AssignedEmployeeId { get; set; } // Yeni alan eklendi
        public Company Company { get; set; } // Bu satır eklendi
    }
}