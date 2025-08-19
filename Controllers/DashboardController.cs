using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.EnvanterLib;
using System.Diagnostics;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;


        [HttpGet]
        public async Task<IActionResult> Index(int? companyId, DateTime? from, DateTime? to)
        {
            var vm = new DashboardViewModel
            {
                CompanyId = companyId,
                FromDate = (from ?? DateTime.UtcNow.AddDays(-30)).Date,
                ToDate = (to ?? DateTime.UtcNow).Date
            };


            vm.Companies = await _db.Companies
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
            .ToListAsync();


            await FillMetricsAsync(vm);
            return View(vm);
        }


        private async Task FillMetricsAsync(DashboardViewModel vm)
        {
            var today = DateTime.UtcNow.Date;
            var from = vm.FromDate;
            var to = vm.ToDate.AddDays(1).AddTicks(-1); // gün sonu


            var computers = _db.Computers.AsNoTracking();
            var software = _db.Software.AsNoTracking();
            var supplies = _db.Supplies.AsNoTracking();
            var logs = _db.ActivityLogs.AsNoTracking();


            if (vm.CompanyId.HasValue)
            {
                int cid = vm.CompanyId.Value;
                computers = computers.Where(x => x.CompanyId == cid);
                software = software.Where(x => x.CompanyId == cid);
                supplies = supplies.Where(x => x.CompanyId == cid);
                // logs tarafında firma bağı varsa ekleyin; yoksa tüm loglardan özet alınabilir
            }


            // Bilgisayar
            vm.TotalComputers = await computers.CountAsync();
            vm.ComputersInUse = await computers.CountAsync(x => x.Status == ComputerStatus.Kullanımda); // Kullanımda);
        }
    }
}