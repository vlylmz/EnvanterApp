
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.ViewModels;
using System.Security.Claims;
using WebApplication1.Services;
using WebApplication1.EnvanterLib;

// Bu sınıf, şirket envanterini yönetmek için gerekli işlemleri içerir LOGLAMA EKLENMİŞTİR

namespace WebApplication1.Controllers
{
    public class CompanyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogger _activityLogger;


        public CompanyController(AppDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;

        }

        // Listele
        public async Task<IActionResult> Index(string search, string status)
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    c.Name!.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "active")
                    query = query.Where(c => c.IsActive);
                else if (status == "passive")
                    query = query.Where(c => !c.IsActive);
            }

            var companies = await query.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Search = search;
            ViewBag.Status = status;
            return View(companies);
        }

        // Detayları gösterme
        public async Task<IActionResult> Details(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            var viewModel = new CompanyDetailsViewModel
            {
                Company = company,
                Employees = await _context.Employees.Where(e => e.CompanyId == id).ToListAsync(),
                Computers = await _context.Computers.Where(c => c.CompanyId == id).ToListAsync(),
                Software = await _context.Software.Where(s => s.CompanyId == id).ToListAsync(),
                Supplies = await _context.Supplies.Where(s => s.CompanyId == id).ToListAsync()
            };

            return View(viewModel); // ← sadece bunu düzelt!
        }
        // Ekle (GET)
        public IActionResult Create()
        {
            return View();
        }
        // Ekle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company model)
        {
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"Company Name: {model.Name}");

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.Companies.Add(model);

                    var result = await _context.SaveChangesAsync();
                    Console.WriteLine("asdasd1");

                    if (result > 0)
                    {
                        // ✅ LOG EKLENDİ
                        await _activityLogger.LogAsync(this.GetUserFromHttpContext()!.Id.ToString(), "Firma oluşturuldu", "Company", model.Id);

                        HttpContext.Session.SetString("successMessage", "Sirket basariyla eklendi.");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Kayıt yapılamadı!";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Hata: {ex.Message}";

                    if (ex.InnerException != null)
                    {
                        TempData["Error"] += $" - İç hata: {ex.InnerException.Message}";
                    }
                }
            }
            else
            {
                TempData["Error"] = "Form validation hatası var!";
            }

            return View(model);
        }

        // Güncelle (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            return View(company);
        }
        // Güncelle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    model.UpdatedDate = DateTime.Now;
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    // ✅ LOG EKLENDİ
                    await _activityLogger.LogAsync(this.GetUserFromHttpContext()!.Id.ToString(), "Firma güncellendi", "Company", model.Id);
                    
                    TempData["Success"] = "Sirket basariyla guncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(model.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Sil (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        // Sil (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();

                // ✅ LOG EKLENDİ
                await _activityLogger.LogAsync(this.GetUserFromHttpContext()!.Id.ToString(), "Firma silindi", "Company", id);

                HttpContext.Session.SetString("successMessage", "Sirket basariyla silindi.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}