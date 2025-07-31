using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.EnvanterLib;

public static class EnvanterLib
{

    public static void SaveUserToHttpContext(this Controller obj, ApplicationUser user)
    {
        obj.HttpContext.Session.Set("userObject", JsonSerializer.SerializeToUtf8Bytes(user));
    }


    public static ApplicationUser? GetUserFromHttpContext(this Controller obj)
    {
        if (obj.HttpContext.Session.TryGetValue("userObject", out var userBytes))
        {
            return JsonSerializer.Deserialize<ApplicationUser>(userBytes);
        }
        return null;
    }


    public static ApplicationUser? GetUserFromHttpContext(HttpContext context)
    {
        if (context.Session.TryGetValue("userObject", out var userBytes))
        {
            return JsonSerializer.Deserialize<ApplicationUser>(userBytes);
        }
        return null;
    }


    public static void TwoFactorHoldUser(this Controller obj, ApplicationUser user)
    {
        obj.HttpContext.Session.Set("TwoFactorHoldUser", JsonSerializer.SerializeToUtf8Bytes(user));
    }


    public static ApplicationUser? GetTwoFactorHoldUser(this Controller obj)
    {
        if (obj.HttpContext.Session.TryGetValue("TwoFactorHoldUser", out var userBytes))
        {
            return JsonSerializer.Deserialize<ApplicationUser>(userBytes);
        }
        return null;
    }


    public static void RemoveTwoFactorHoldUser(this Controller obj)
    {
        obj.HttpContext.Session.Remove("TwoFactorHoldUser");
    }

}

