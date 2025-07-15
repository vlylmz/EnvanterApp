using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class User
    {
        public int id { get; set; }

        public string? userName { get; set; }

        public string? realName { get; set; }

        public string? surname { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime registerDate { get; set; }

        public byte[]? totpSecret {  get; set; }


    }
}
