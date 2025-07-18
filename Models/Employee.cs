using System.ComponentModel.DataAnnotations;
namespace WebApplication1.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int CompanyId { get; set; }

        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  

        [Required]
        public string Password { get; set; } = "Manisa.45";
    }
}
