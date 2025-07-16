namespace WebApplication1.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AssetTag { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}