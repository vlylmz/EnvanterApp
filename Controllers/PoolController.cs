using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

public class PoolController : Controller
{
    private readonly AppDbContext _context;

    public PoolController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? companyId)
    {
        // Sadece havuzda olan bilgisayarlar
        var poolItems = _context.Computers.Where(x => x.Status == "InPool");

        if (companyId.HasValue)
            poolItems = poolItems.Where(x => x.CompanyId == companyId.Value);

        var computers = await poolItems.ToListAsync();
        ViewBag.Companies = await _context.Companies.ToListAsync();
        ViewBag.SelectedCompany = companyId;
        return View(computers);
    }

    public async Task<IActionResult> SetInUse(int id)
    {
        var pc = await _context.Computers.FindAsync(id);
        if (pc != null)
        {
            pc.Status = "InUse";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Assign(int id)
    {
        var computer = await _context.Computers.FindAsync(id);
        if (computer == null) return NotFound();

        var employees = await _context.Employees.Where(e => e.CompanyId == computer.CompanyId).ToListAsync();
        ViewBag.Employees = employees;
        return View(computer);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(int id, int employeeId)
    {
        var computer = await _context.Computers.FindAsync(id);
        var employee = await _context.Employees.FindAsync(employeeId);

        if (computer != null && employee != null && computer.CompanyId == employee.CompanyId)
        {
            computer.Status = "InUse";
            computer.AssignedEmployeeId = employee.Id;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    public IActionResult Create()
    {
        ViewBag.Companies = _context.Companies.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Computer model)
    {
        if (ModelState.IsValid)
        {
            model.Status = "InPool";
            _context.Computers.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        ViewBag.Companies = await _context.Companies.ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var computer = await _context.Computers.FindAsync(id);
        if (computer == null)
            return NotFound();

        return View(computer);
    }
}
