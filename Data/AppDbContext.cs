
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // İleride eklenecek diğer DbSet'ler:
        //DbSet<Company> Companies için db bağlantısı   
        public DbSet<Company> Companies { get; set; }
        // db bağlantısı Employee, Computer, Software ve Supply için
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Software> Software { get; set; }
        public DbSet<Supply> Supplies { get; set; }


        // public DbSet<Employee> Employees { get; set; }
    }
}
