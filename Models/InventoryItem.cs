namespace WebApplication1.Models
{
    public abstract class InventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public InventoryStatus Status { get; set; }
        public int? AssignedEmployeeId { get; set; }
        public Employee AssignedEmployee { get; set; }
        public string AssetTag { get; set; }         // Varlık Etiketi
        public string SerialNumber { get; set; }     // Seri numarası
        public DateTime CreatedDate { get; set; }
    }
}
