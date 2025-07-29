
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Software> Software { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<ItemModel> Items { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

    }
}
