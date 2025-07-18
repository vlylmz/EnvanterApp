using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1
{
    public class EnvanterLib
    {
        public static User? GetUserFromSession(Controller c)
        {
            var userJson = c.HttpContext.Session.GetString("userJson");
            if (userJson != null)
                return JsonSerializer.Deserialize<User>(userJson);
            return null;
        }
    }
}
