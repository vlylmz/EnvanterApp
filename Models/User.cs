using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; } = null!;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; }

        public byte[]? TotpSecret {  get; set; }

        [Required]
        public string PasswordHash { get; set; } = null!;
    }
}
