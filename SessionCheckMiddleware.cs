using WebApplication1.EnvanterLib;

public class SessionCheckMiddleware
{
    private readonly RequestDelegate _next;

    public SessionCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var routeData = context.GetRouteData();
        var controller = routeData.Values["controller"]?.ToString()?.ToLower();
        var action = routeData.Values["action"]?.ToString()?.ToLower();

        Console.WriteLine(controller + ":" + action);

        if (controller == "login" && (action == "ındex" || action == "qrvalidate"))
        {
            await _next(context);
            return;
        }

        if (EnvanterLib.GetUserFromHttpContext(context) == null)
        {
            context.Response.Redirect("/Login/Index");
            return;
        }

        await _next(context);
        
    }
}