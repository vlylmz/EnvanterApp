using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public enum UserRoles
    {
        SuperAdmin,
        Admin,
        User
    }

    public class ApplicationUser
    {
        [Required]
        public int Id { get; set; }


        [Required]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Email { get; set; }


        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;


        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public UserRoles? UserRole { get; set; }


        public long PhoneNumber { get; set; }


        public byte[]? TotpSecret { get; set; }
    }
}
