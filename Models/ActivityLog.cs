using System.ComponentModel.DataAnnotations;
namespace WebApplication1.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!; 
        
        [Required]
        [StringLength(200)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }  // Navigation property
    }
}