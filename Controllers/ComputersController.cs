using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Bu satırı ekleyin
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class ComputersController : Controller
    {
        private readonly AppDbContext _context;

        public ComputersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Computers
        public async Task<IActionResult> Index()
        {
            var computers = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .ToListAsync();

            return View(computers);
        }

        // GET: Computers/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Computers/Create
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
            catch (Exception ex)
            {
                // Hata durumunda boş listeler ile devam et
                ViewBag.Companies = new List<SelectListItem>();
                ViewBag.Employees = new List<SelectListItem>();
                return View(new Computer());
            }
        }

        // POST: Computers/Create
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
                    
                    TempData["Success"] = "Bilgisayar başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Bilgisayar eklenirken hata oluştu: " + ex.Message;
                }
            }

            // Hata durumunda ViewBag verilerini tekrar doldur
            try
            {
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
            }
            catch (Exception)
            {
                ViewBag.Companies = new List<SelectListItem>();
                ViewBag.Employees = new List<SelectListItem>();
            }

            return View(computer);
        }

        // GET: Computers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
                return NotFound();

            // ViewBag verilerini SelectListItem olarak doldur
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

        // POST: Computers/Edit/5
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
                    computer.LastUpdatedDate = DateTime.UtcNow;
                    _context.Update(computer);
                    await _context.SaveChangesAsync();
                    
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

            // Hata durumunda ViewBag verilerini tekrar doldur
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

        // GET: Computers/Delete/5
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

        // POST: Computers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var computer = await _context.Computers.FindAsync(id);
            if (computer != null)
            {
                _context.Computers.Remove(computer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bilgisayar başarıyla silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ComputerExists(int id)
        {
            return _context.Computers.Any(e => e.Id == id);
        }
    }
}