using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Models;

public class ComputerController : Controller
{
    private readonly AppDbContext _context;

    public ComputerController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.Computers.ToListAsync();
        return View(list);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Computer model)
    {
        if (ModelState.IsValid)
        {
            _context.Computers.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var pc = await _context.Computers.FindAsync(id);
        if (pc == null) return NotFound();
        return View(pc);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Computer model)
    {
        if (ModelState.IsValid)
        {
            _context.Computers.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var pc = await _context.Computers.FindAsync(id);
        if (pc != null)
        {
            _context.Computers.Remove(pc);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(int id)
    {
        var pc = await _context.Computers.FindAsync(id);
        if (pc == null) return NotFound();
        return View(pc);
    }
}
