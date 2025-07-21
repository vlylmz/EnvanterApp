using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

public class EmployeeController : Controller

{   
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchName, int? companyId, string status)
    {
        var employees = _context.Employees
            .Include(e => e.Company)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchName))
            employees = employees.Where(e => e.FirstName.Contains(searchName) || e.LastName.Contains(searchName));

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
        // ModelState debug için
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
                TempData["Success"] = "Çalışan başarıyla eklendi!";
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

    //public async Task<IActionResult> Edit(int id)
    //{
    //    var emp = await _context.Employees.FindAsync(id);
    //    if (emp == null) return NotFound();

    //    // Edit için de ViewBag.Companies gerekli
    //    ViewBag.Companies = await _context.Companies
    //        .Where(c => c.IsActive)
    //        .OrderBy(x => x.Name)
    //        .ToListAsync();

    //    return View(emp);
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Edit(Employee model)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        try
    //        {
    //            _context.Employees.Update(model);
    //            await _context.SaveChangesAsync();
    //            TempData["Success"] = "Çalışan başarıyla güncellendi!";
    //            return RedirectToAction("Index");
    //        }
    //        catch (Exception ex)
    //        {
    //            TempData["Error"] = "Güncelleme sırasında hata oluştu: " + ex.Message;
    //        }
    //    }

    //    // Hata durumunda ViewBag'i tekrar doldur
    //    ViewBag.Companies = await _context.Companies
    //        .Where(c => c.IsActive)
    //        .OrderBy(x => x.Name)
    //        .ToListAsync();
    //    return View(model);
    //}

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp != null)
            {
                _context.Employees.Remove(emp);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Çalışan başarıyla silindi!";
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

        ViewBag.CompanyList = new SelectList(_context.Companies, "Id", "Name", employee.CompanyId);
        return View(employee);
    }


}