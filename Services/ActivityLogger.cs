using WebApplication1.Data;
using WebApplication1.Models;
using System.Threading.Tasks;
using System.Text.Json;

namespace WebApplication1.Services
{
    public class ActivityLogger : IActivityLogger
    {
        private readonly AppDbContext _context;

        public ActivityLogger(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int userId, string action, string entityType, int entityId, string? detail = null)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Detail = detail,
                CreatedDate = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

    public static class LogHelper
    {
       public static string GetDifferences<T>(T oldObj, T newObj)
{
    var diffs = new List<string>();
    foreach (var prop in typeof(T).GetProperties())
    {
        if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
            continue;

        if (prop.Name is "UpdatedDate" or "UpdatedBy" or "CreatedDate" or "CreatedBy")
            continue;

        var oldVal = prop.GetValue(oldObj)?.ToString() ?? "";
        var newVal = prop.GetValue(newObj)?.ToString() ?? "";

        if (oldVal != newVal)
        {
            diffs.Add($"{prop.Name}: \"{oldVal}\" → \"{newVal}\"");
        }
    }

    return diffs.Count > 0 ? string.Join(",\n", diffs) : "Hiçbir alan değişmedi.";
}

        public static string GetSummary<T>(T entity)
        {
            var props = typeof(T).GetProperties();
            var summaryParts = new List<string>();

            foreach (var prop in props)
            {
                if (prop.Name is "Id" or "CreatedDate" or "CreatedBy" or "UpdatedDate" or "UpdatedBy")
                    continue;

                var value = prop.GetValue(entity)?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    summaryParts.Add($"{prop.Name}: {value}\n");
                }
            }

            return string.Join(", \n", summaryParts);
        }

    }
}