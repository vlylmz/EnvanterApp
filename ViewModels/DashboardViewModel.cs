using Microsoft.AspNetCore.Mvc.Rendering;


namespace WebApplication1.ViewModels
{
public class DashboardViewModel
{
// Filtreler
public int? CompanyId { get; set; }
public DateTime FromDate { get; set; } = DateTime.UtcNow.AddDays(-30);
public DateTime ToDate { get; set; } = DateTime.UtcNow;


// Kartlar / Sayılar
public int TotalAssets { get; set; }
public int TotalComputers { get; set; }
public int ComputersInUse { get; set; }
public int ComputersInPool { get; set; }


public int TotalSoftware { get; set; }
public int LicenseExpiring30d { get; set; }
public int LicenseExpiring7d { get; set; }
public int LicenseExpired { get; set; }


public int TotalSupplies { get; set; }
public int CriticalSupplies { get; set; }


public int TotalAssignments { get; set; }
public int AssignmentsLast7d { get; set; }

    
public Dictionary<string,int> PoolByCompany { get; set; } = new();
public List<(DateTime date, int count)> AssignmentsTrend { get; set; } = new();


public List<ActivityItem> RecentActivities { get; set; } = new();
public List<SelectListItem> Companies { get; set; } = new();
}


public class ActivityItem
{
public DateTime CreatedDate { get; set; }
public string UserName { get; set; } = string.Empty;
public string Action { get; set; } = string.Empty;
public string EntityType { get; set; } = string.Empty;
public int EntityId { get; set; }
}
}