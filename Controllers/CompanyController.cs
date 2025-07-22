﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class CompanyController : Controller
    {
        private readonly AppDbContext _context;

        public CompanyController(AppDbContext context)
        {
            _context = context;
        }

        // Listele
        public async Task<IActionResult> Index(string search, string status)
        {
            var query = _context.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(search) ||
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
            // Debug için
        Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
        Console.WriteLine($"Company Name: {model.Name}");
        if (ModelState.IsValid)
        {
            try
            {
                model.CreatedDate = DateTime.Now;
                _context.Companies.Add(model);

                // SaveChanges'tan önce kaç kayıt etkileneceğini kontrol et
                var result = await _context.SaveChangesAsync();

                // Eğer result 0 ise kayıt olmamış demektir
                if (result > 0)
                {
                    TempData["Success"] = "Şirket başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Kayıt yapılamadı!";
                }
            }
            catch (Exception ex)
            {
                // Hata mesajını TempData ile göster
                TempData["Error"] = $"Hata: {ex.Message}";

                // Inner exception varsa onu da ekle
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" - İç hata: {ex.InnerException.Message}";
                }
            }
        }
        else
        {
            // Model validation hatalarını göster
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
                    TempData["Success"] = "Şirket başarıyla güncellendi.";
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
                TempData["Success"] = "Şirket başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}