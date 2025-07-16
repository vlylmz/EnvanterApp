namespace WebApplication1.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AssetTag { get; set; }
        public string Status { get; set; }
<<<<<<< HEAD
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public DateTime PurchaseDate { get; set; }
=======
        public string SerialNumber { get; set; }
        public int CompanyId { get; set; }
        public int AssignedEmployeeId { get; set; }
        public DateTime CreatedDate { get; set; }
        

>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
    }
}
