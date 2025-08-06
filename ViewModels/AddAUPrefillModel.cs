namespace WebApplication1.Models
{
    public class AddAUPrefillModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public long PhoneNumber { get; set; }
        public UserRoles UserRole { get; set; } = UserRoles.User;
        public string? WelcomeMessage { get; set; }


    }
}