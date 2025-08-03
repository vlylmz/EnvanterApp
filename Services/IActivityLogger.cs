using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface IActivityLogger
    {
    Task LogAsync(int userId, string action, string entityType, int entityId, string? detail = null);
    }
}