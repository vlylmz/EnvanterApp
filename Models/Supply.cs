using System;
using WebApplication1.Services;
namespace WebApplication1.Models
{ 
public class Supply : IHasLogs
{
    public int Id { get; set; }

    public string? Name { get; set; }
    public string? SystemBarcode { get; set; }

    public int CompanyId { get; set; }
    public int? AssignedEmployeeId { get; set; }

    public string? Status { get; set; } // "InUse", "InPool", vb.
}
}