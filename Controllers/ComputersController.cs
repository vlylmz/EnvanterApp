using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.EnvanterLib;
using System.Diagnostics;

namespace WebApplication1.Controllers
{
    public class ComputersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public ComputersController(AppDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;
        }


        public async Task<IActionResult> Index()
        {

            var computers = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .ToListAsync();


            return View(computers);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .Include(c => c.ActivityLogs)
                .ThenInclude(ct => ct.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (computer == null)
                return NotFound();

            return View(computer);
        }


        public async Task<IActionResult> Create()
        {
            try
            {
                // ViewBag verilerini doldur
                ViewBag.Companies = await _context.Companies
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                ViewBag.Employees = await _context.Employees
                    .Select(e => new SelectListItem
                    {
                        Value = e.Id.ToString(),
                        Text = $"{e.FirstName} {e.LastName}"
                    })
                    .ToListAsync();

                // Boş model döndür
                return View(new Computer());
            }
            catch (Exception)
            {
                Debug.Assert(false);
                // Hata durumunda boş listeler ile devam et
                ViewBag.Companies = new List<SelectListItem>();
                ViewBag.Employees = new List<SelectListItem>();
                return View(new Computer());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Computer computer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    computer.CreatedDate = DateTime.UtcNow;
                    _context.Computers.Add(computer);
                    await _context.SaveChangesAsync();

                    var detail = LogHelper.GetSummary(computer);
                    await _activityLogger.LogAsync(this.GetUserFromHttpContext()!.Id, "Bilgisayar oluşturuldu", "Computer", computer.Id, detail, computer);

                    TempData["Success"] = "Bilgisayar başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Bilgisayar eklenirken hata oluştu: " + ex.Message;
                    Debug.Assert(false);

                }
            }

            // Hata durumunda ViewBag verilerini tekrar doldur
            ViewBag.Companies = await _context.Companies
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Employees = await _context.Employees
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" })
                .ToListAsync();

            TempData["Error"] = "error at Create()";

            return View(computer);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
                return NotFound();

            ViewBag.Companies = await _context.Companies
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            ViewBag.Employees = await _context.Employees
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.FirstName} {e.LastName}"
                })
                .ToListAsync();

            return View(computer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Computer computer)
        {
            if (id != computer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var original = await _context.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (original == null)
                        return NotFound();

                    computer.LastUpdatedDate = DateTime.UtcNow;
                    _context.Update(computer);
                    await _context.SaveChangesAsync();

                    var detail = LogHelper.GetDifferences(original, computer);
                    await _activityLogger.LogAsync(this.GetUserFromHttpContext()!.Id, "Bilgisayar güncellendi", "Computer", computer.Id, detail, computer);

                    TempData["Success"] = "Bilgisayar başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComputerExists(computer.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Companies = await _context.Companies
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Employees = await _context.Employees
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = $"{e.FirstName} {e.LastName}" })
                .ToListAsync();

            return View(computer);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (computer == null)
                return NotFound();

            return View(computer);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var computer = await _context.Computers.FindAsync(id);
            if (computer != null)
            {
                // Soft delete
                computer.IsActive = false;
                computer.LastUpdatedDate = DateTime.UtcNow;
                computer.LastUpdatedBy = User.Identity?.Name ?? "Unknown";

                await _context.SaveChangesAsync();

                var detail = $"Bilgisayar pasife alındı (ID: {computer.Id}, Ad: {computer.Name})";
                await _activityLogger.LogAsync(this.GetUserFromHttpContext()!.Id, "Bilgisayar pasife alındı", "Computer", computer.Id, detail, computer);

                TempData["Success"] = "Bilgisayar başarıyla pasif duruma alındı.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ComputerExists(int id)
        {
            return _context.Computers.Any(e => e.Id == id);
        }

    }
}