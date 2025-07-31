using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Security.Claims;
using WebApplication1.EnvanterLib;

namespace WebApplication1.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public EmployeeController(AppDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;

        }

        public async Task<IActionResult> Index(string searchName, int? companyId, string status)
        {
            var employees = _context.Employees
                .Include(e => e.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
                employees = employees.Where(e => e.FirstName != null && (e.FirstName.Contains(searchName) || (e.LastName != null && e.LastName.Contains(searchName))));

            if (companyId.HasValue && companyId > 0)
                employees = employees.Where(e => e.CompanyId == companyId);

            if (!string.IsNullOrEmpty(status))
                employees = employees.Where(e => (status == "Aktif" && e.IsActive) || (status == "Pasif" && !e.IsActive));

            // Dropdown için şirket listesini ViewBag ile gönder
            ViewBag.Companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(await employees.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model)
        {
            Console.WriteLine(">>> Create POST çağrıldı");

            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        Console.WriteLine($"Key: {modelError.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.UtcNow;
                    _context.Employees.Add(model);
                    await _context.SaveChangesAsync();

                    // LOG EKLENDİ
                    string? userId = this.GetUserFromHttpContext()?.Id.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _activityLogger.LogAsync(userId, "Çalışan oluşturuldu", "Employee", model.Id);
                    }

                    TempData["Success"] = "Calisan basariyla eklendi!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Çalışan eklenirken bir hata oluştu: " + ex.Message;
                }
            }

            // Hata durumunda ViewBag'i tekrar doldur
            ViewBag.Companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(model);
        }

     [HttpPost]
public async Task<IActionResult> Delete(int id)
{
    try
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp != null)
        {
            emp.IsActive = false;
            emp.ActiveTime = DateTimeOffset.UtcNow;

            _context.Employees.Update(emp);
            await _context.SaveChangesAsync();

            // LOG EKLENDİ
            string? userId = this.GetUserFromHttpContext()?.Id.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                var detail = $"Pasif duruma geçirildi: {emp.FirstName} {emp.LastName}, Email: {emp.Email}";
                await _activityLogger.LogAsync(userId, "Çalışan pasifleştirildi", "Employee", emp.Id, detail);
            }

            TempData["Success"] = "Çalışan başarıyla pasifleştirildi!";
        }
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Silme işlemi sırasında hata oluştu: " + ex.Message;
    }

    return RedirectToAction("Index");
}


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                TempData["Error"] = "Çalışan bulunamadı.";
                return RedirectToAction("Index");
            }

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                TempData["Error"] = "Çalışan bulunamadı.";
                return RedirectToAction("Index");
            }

            ViewBag.CompanyId = new SelectList(_context.Companies, "Id", "Name", employee.CompanyId);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    // LOG EKLENDİ
                    string? userId = this.GetUserFromHttpContext()?.Id.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _activityLogger.LogAsync(userId, "Çalışan güncellendi", "Employee", model.Id);
                    }

                    TempData["Success"] = "Çalışan başarıyla güncellendi!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.Id == model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Re-populate dropdown on error
            ViewBag.CompanyId = new SelectList(_context.Companies, "Id", "Name", model.CompanyId);
            return View(model);
        }
    }
}