namespace WebApplication1.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AssetTag { get; set; }
        public string Status { get; set; }
        public string SerialNumber { get; set; }
        public int CompanyId { get; set; }
        public int AssignedEmployeeId { get; set; }
        public DateTime CreatedDate { get; set; }
        

    }
}
