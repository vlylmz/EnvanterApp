using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IHasLogs
    {
        void AddtoOwnLogs(ActivityLog log);
    }
}