using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1
{
    public class EnvanterLib
    {
        public static User? getUserFromSession(Controller c)
        {
            var userJson = c.HttpContext.Session.GetString("userJson");
            if (userJson == null || string.IsNullOrEmpty(userJson))
                return null;
            return JsonSerializer.Deserialize<User>(userJson);
        }
    }
}
