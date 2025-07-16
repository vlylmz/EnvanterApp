using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        
        public string? Email { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime CreatedTime { get; set; }

        public byte[]? TotpSecret {  get; set; }

        public string? PasswordHash { get; set; }
    }
}
