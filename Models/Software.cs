using System;
namespace WebApplication1.Models
{
    public class Software
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Brand { get; set; }

        public int CompanyId { get; set; }
        public int? AssignedEmployeeId { get; set; }

        public DateTime PurchaseDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public string Status { get; set; } // "Aktif", "Yaklaşan", "Süresi Dolmuş"
    }
}