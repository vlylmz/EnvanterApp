using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IActivityLogger, ActivityLogger>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseMiddleware<SessionCheckMiddleware>();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}");


var dbC = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
if (dbC.Users.Any(u => u.UserName == "admin") == false)
{
    Console.WriteLine("Admin user not found, creating one...");
    dbC.Users.Add(new ApplicationUser
    {
        FirstName = "Hakkı",
        LastName = "Alkan",
        Email = "",
        UserName = "admin",
        UserRole = ""
    });

    dbC.SaveChanges();
}

app.Run();