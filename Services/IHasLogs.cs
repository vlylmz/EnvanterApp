using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class IHasLogs
    {
        public List<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public void AddtoOwnLogs(ActivityLog log)
        {
            ActivityLogs.Add(log);
        }
    }
}