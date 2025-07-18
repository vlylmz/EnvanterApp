using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
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


        // Ekle (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Ekle (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Company model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Companies.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, Company model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            _context.Companies.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        //Detayları gösterme
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

            return View(viewModel);
        }


    }
}
