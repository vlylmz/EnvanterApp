using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // AppDbContext için
using WebApplication1.Models; // ApplicationUser için

var builder = WebApplication.CreateBuilder(args);

// 🔗 1. Veritabanı Bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔐 2. Identity Servisleri
/*builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();*/

// 🛡️ 3. Cookie Ayarları
/*builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});*/

// 4. MVC Controller + Razor
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 📦 Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

/*app.UseAuthentication(); // <<< Giriş kontrolü
app.UseAuthorization();*/


//app.MapControllerRoute(
// name: "default",
// pattern: "{controller=Account}/{action=Login}/{id?}");


//app.MapControllerRoute(
   // name: "default",
   // pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Gerekli roller
    string[] roles = ["Süper Admin", "Admin", "Employee"];

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Süper Admin kullanıcı
    string email = "admin@spiltech.com";
    string password = "Admin123!"; // şifre politikası uymalı
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Süper",
            LastName = "Admin",
            UserRole = "Süper Admin",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(newUser, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, "Süper Admin");
        }
    }
}*/

app.Run();
