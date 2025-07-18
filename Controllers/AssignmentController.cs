using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

public class AssignmentController : Controller
{
    private readonly AppDbContext _context;

    public AssignmentController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var assigned = await _context.Computers
            .Where(c => c.Status == "InUse" && c.AssignedEmployeeId.HasValue)
            .Include(c => c.CompanyId)
            .ToListAsync();

        var employees = await _context.Employees.ToListAsync();

        var viewList = assigned.Select(c =>
        {
            var emp = employees.FirstOrDefault(e => e.Id == c.AssignedEmployeeId);
            return new
            {
                c.Id,
                c.Name,
                c.AssetTag,
                c.Status,
                AssignedUserFullName = emp != null ? (emp.FirstName + " " + emp.LastName) : "-",
                AssignedUserEmail = emp?.Email ?? "-"
            };
        }).ToList();

        return View(viewList);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Computers = await _context.Computers.Where(c => c.Status != "InUse").ToListAsync();
        ViewBag.Employees = await _context.Employees.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(int computerId, int employeeId)
    {
        var computer = await _context.Computers.FindAsync(computerId);
        var employee = await _context.Employees.FindAsync(employeeId);

        if (computer != null && employee != null)
        {
            computer.Status = "InUse";
            computer.AssignedEmployeeId = employee.Id;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Unassign(int id)
    {
        var computer = await _context.Computers.FindAsync(id);
        if (computer != null)
        {
            computer.Status = "InPool";
            computer.AssignedEmployeeId = null;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}
