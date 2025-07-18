using System;
namespace WebApplication1.Models
{
    public class Computer
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }

        public int CompanyId { get; set; }
        public int? AssignedEmployeeId { get; set; }

        public string Status { get; set; } // "InUse", "InPool"
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}