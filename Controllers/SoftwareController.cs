using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Data;
namespace WebApplication1.Controllers
{
    public class SoftwareController : Controller
    {
        private readonly AppDbContext _context;
        
        public SoftwareController(AppDbContext context)
        {
            _context = context;
        }

        // Software listesi (GET)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var softwareList = await _context.Software
                .Include(s => s.AssignedEmployee)
                .Include(s => s.Company)
                .ToListAsync();
                
            // Her yazılım için durumu güncelle
            foreach (var software in softwareList)
            {
                software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
                software.Status = DetermineStatus(software.ExpiryDate);
                software.AlertColor = DetermineAlertColor(software.Status);
            }
            
            return View(softwareList);
        }

        // Software ekleme sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // Software ekleme işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Software software)
        {
            if (ModelState.IsValid)
            {
                // Kalan süreyi hesapla
                software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
                
                // Durumu belirle
                software.Status = DetermineStatus(software.ExpiryDate);
                
                // Görsel uyarı rengini belirle
                software.AlertColor = DetermineAlertColor(software.Status);
                
                // Oluşturma tarihini set et
                software.CreatedDate = DateTime.Now;

                // Veritabanına kaydet
                _context.Software.Add(software);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Yazılım lisansı başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            
            // Model geçerli değilse dropdown verilerini tekrar yükle
            await LoadDropdownData();
            return View(software);
        }

        // Software düzenleme sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var software = await _context.Software
                .Include(s => s.AssignedEmployee)
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (software == null)
            {
                TempData["ErrorMessage"] = "Yazılım lisansı bulunamadı.";
                return RedirectToAction("Index");
            }
            
            return View(software);
        }

        // Software düzenleme işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Software software)
        {
            if (ModelState.IsValid)
            {
                // Kalan süreyi hesapla
                software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
                
                // Durumu belirle
                software.Status = DetermineStatus(software.ExpiryDate);
                
                // Görsel uyarı rengini belirle
                software.AlertColor = DetermineAlertColor(software.Status);
                
                // Güncelleme tarihini set et
                software.UpdatedDate = DateTime.Now;

                // Veritabanında güncelle
                _context.Software.Update(software);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Yazılım lisansı başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            
            return View(software);
        }

        // Software silme işlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var software = await _context.Software.FindAsync(id);
            if (software == null)
            {
                TempData["ErrorMessage"] = "Yazılım lisansı bulunamadı.";
                return RedirectToAction("Index");
            }

            // Veritabanından sil
            _context.Software.Remove(software);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Yazılım lisansı başarıyla silindi.";
            return RedirectToAction("Index");
        }

        // Software detay sayfası (GET)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var software = await _context.Software
                .Include(s => s.AssignedEmployee)
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (software == null)
            {
                TempData["ErrorMessage"] = "Yazılım lisansı bulunamadı.";
                return RedirectToAction("Index");
            }
            
            // Durumu güncelle
            software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
            software.Status = DetermineStatus(software.ExpiryDate);
            software.AlertColor = DetermineAlertColor(software.Status);
            
            return View(software);
        }

        // Süresi dolmak üzere olan lisanslar
        [HttpGet]
        public async Task<IActionResult> Expiring()
        {
            var expiringSoftware = await _context.Software
                .Include(s => s.AssignedEmployee)
                .Include(s => s.Company)
                .Where(s => s.ExpiryDate > DateTime.Now && 
                           s.ExpiryDate <= DateTime.Now.AddDays(30))
                .ToListAsync();
                
            // Durumları güncelle
            foreach (var software in expiringSoftware)
            {
                software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
                software.Status = DetermineStatus(software.ExpiryDate);
                software.AlertColor = DetermineAlertColor(software.Status);
            }
            
            return View(expiringSoftware);
        }

        // Süresi dolmuş lisanslar
        [HttpGet]
        public async Task<IActionResult> Expired()
        {
            var expiredSoftware = await _context.Software
                .Include(s => s.AssignedEmployee)
                .Include(s => s.Company)
                .Where(s => s.ExpiryDate < DateTime.Now)
                .ToListAsync();
                
            // Durumları güncelle
            foreach (var software in expiredSoftware)
            {
                software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
                software.Status = DetermineStatus(software.ExpiryDate);
                software.AlertColor = DetermineAlertColor(software.Status);
            }
            
            return View(expiredSoftware);
        }

        // AJAX ile filtreleme
        [HttpGet]
        public async Task<IActionResult> Filter(string searchTerm, string status)
        {
            var query = _context.Software
                .Include(s => s.AssignedEmployee)
                .Include(s => s.Company)
                .AsQueryable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm) || 
                                        s.Brand.Contains(searchTerm) ||
                                        (s.Description != null && s.Description.Contains(searchTerm)));
            }

            // Durum filtresi
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<SoftwareStatus>(status, out var statusEnum))
            {
                switch (statusEnum)
                {
                    case SoftwareStatus.Active:
                        query = query.Where(s => s.ExpiryDate > DateTime.Now.AddDays(30));
                        break;
                    case SoftwareStatus.Approaching:
                        query = query.Where(s => s.ExpiryDate > DateTime.Now && 
                                               s.ExpiryDate <= DateTime.Now.AddDays(30));
                        break;
                    case SoftwareStatus.Expired:
                        query = query.Where(s => s.ExpiryDate < DateTime.Now);
                        break;
                }
            }

            var filteredSoftware = await query.ToListAsync();
            
            // Durumları güncelle
            foreach (var software in filteredSoftware)
            {
                software.RemainingTime = CalculateRemainingTime(software.ExpiryDate);
                software.Status = DetermineStatus(software.ExpiryDate);
                software.AlertColor = DetermineAlertColor(software.Status);
            }

            return PartialView("_SoftwareTablePartial", filteredSoftware);
        }

        // Hata sayfası
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string CalculateRemainingTime(DateTime expiryDate)
        {
            var remainingTime = expiryDate - DateTime.Now;
            
            if (remainingTime.TotalDays < 0)
                return $"{Math.Abs((int)remainingTime.TotalDays)} gün geçti";
            else if (remainingTime.TotalDays < 30)
                return $"{(int)remainingTime.TotalDays} gün kaldı";
            else if (remainingTime.TotalDays < 365)
                return $"{(int)(remainingTime.TotalDays / 30)} ay kaldı";
            else
                return $"{(int)(remainingTime.TotalDays / 365)} yıl kaldı";
        }

        private SoftwareStatus DetermineStatus(DateTime expiryDate)
        {
            var remainingDays = (expiryDate - DateTime.Now).TotalDays;
            
            if (remainingDays < 0)
                return SoftwareStatus.Expired;
            else if (remainingDays <= 30)
                return SoftwareStatus.Approaching;
            else
                return SoftwareStatus.Active;
        }

        private AlertColor DetermineAlertColor(SoftwareStatus status)
        {
            return status switch
            {
                SoftwareStatus.Expired => AlertColor.Red,
                SoftwareStatus.Approaching => AlertColor.Yellow,
                SoftwareStatus.Active => AlertColor.Green,
                _ => AlertColor.Green
            };
        }

        private async Task LoadDropdownData()
        {
            // Şirketleri yükle
            var companies = await _context.Companies
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();
            ViewBag.Companies = companies;

            // Çalışanları yükle
            var employees = await _context.Employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new { 
                    Id = e.Id, 
                    Name = e.FirstName + " " + e.LastName 
                })
                .ToListAsync();
            ViewBag.Employees = employees;
        }
    }
}