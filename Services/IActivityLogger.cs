using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IActivityLogger
    {
        Task LogAsync<T>(int userId, string action, string entityType, int entityId, string? detail = null, T? logToThisToo = null) where T : IHasLogs;
        Task LogAsync(int userId, string action, string entityType, int entityId, string? detail = null);
        
    }
}