public class Software
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Brand { get; set; }
    public int CompanyId { get; set; }

    // In-memory için kolaylık olsun diye bunları ekle
    public string CompanyName { get; set; } // <-- Ekledik!
    public int? AssignedEmployeeId { get; set; }
    public string AssignedEmployeeName { get; set; } // <-- Ekledik!

    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; }
}
