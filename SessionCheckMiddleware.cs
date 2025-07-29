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
        //var action = routeData.Values["action"]?.ToString()?.ToLower();

        if (controller == "login")
        {
            await _next(context);
            return;
        }

        if (context.Session.GetString("user") != "admin")
        {
            context.Response.Redirect("/Login/Index");
            return;
        }

        await _next(context);
        
    }
}