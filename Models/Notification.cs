namespace WebApplication1.Models
{
    
    public class Notification
    {
        public int Id { get; set; }
        public bool Unread { get; set; } = true;
        public String Content { get; set; } = "";
    }

}