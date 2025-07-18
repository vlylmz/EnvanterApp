using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

public class SoftwareController : Controller
{
    private readonly AppDbContext _context;

    public SoftwareController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search, int? companyId, int? assignedEmployeeId, string status)
    {
        var list = _context.Software.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            list = list.Where(s => s.Name.Contains(search) || s.Brand.Contains(search));
        if (companyId.HasValue)
            list = list.Where(s => s.CompanyId == companyId.Value);
        if (assignedEmployeeId.HasValue)
            list = list.Where(s => s.AssignedEmployeeId == assignedEmployeeId.Value);
        if (!string.IsNullOrEmpty(status))
            list = list.Where(s => s.Status == status);

        var softwareList = await list.ToListAsync();

        // Lisans durumunu güncelle
        foreach (var sw in softwareList)
        {
            sw.Status = GetStatus(sw.ExpiryDate);
        }

        // Dropdownlar için şirket ve çalışan listesi
        ViewBag.Companies = await _context.Companies
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
        ViewBag.Employees = await _context.Employees
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToListAsync();

        return View(softwareList);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Companies = await _context.Companies
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
        ViewBag.Employees = await _context.Employees
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Software model)
    {
        if (ModelState.IsValid)
        {
            model.Status = GetStatus(model.ExpiryDate);
            _context.Software.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        ViewBag.Companies = await _context.Companies
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
        ViewBag.Employees = await _context.Employees
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sw = await _context.Software.FindAsync(id);
        if (sw == null) return NotFound();
        return View(sw);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var sw = await _context.Software.FindAsync(id);
        if (sw != null)
        {
            _context.Software.Remove(sw);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var sw = await _context.Software.FindAsync(id);
        if (sw == null) return NotFound();

        ViewBag.Companies = await _context.Companies
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
        ViewBag.Employees = await _context.Employees
            .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToListAsync();
        return View(sw);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Software model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Companies = await _context.Companies
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
            ViewBag.Employees = await _context.Employees
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.FirstName + " " + e.LastName }).ToListAsync();
            return View(model);
        }

        var sw = await _context.Software.FindAsync(model.Id);
        if (sw == null) return NotFound();

        sw.Name = model.Name;
        sw.Brand = model.Brand;
        sw.CompanyId = model.CompanyId;
        sw.AssignedEmployeeId = model.AssignedEmployeeId;
        sw.PurchaseDate = model.PurchaseDate;
        sw.ExpiryDate = model.ExpiryDate;
        sw.Status = GetStatus(model.ExpiryDate);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    private string GetStatus(DateTime expiryDate)
    {
        var kalanGun = (expiryDate - DateTime.Today).TotalDays;
        if (kalanGun < 0)
            return "Süresi Dolmuş";
        if (kalanGun <= 30)
            return "Yaklaşan";
        return "Aktif";
    }
}
