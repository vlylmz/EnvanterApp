using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class ComputerController : Controller
    {
        private static List<Computer> _computers = new List<Computer>
        {
            new Computer { Id = 1, Name = "PC-001", CreatedDate = DateTime.Now.AddMonths(-6) },
            new Computer { Id = 2, Name = "PC-002", CreatedDate = DateTime.Now.AddMonths(-2) },
            new Computer { Id = 3, Name = "PC-003", CreatedDate = DateTime.Now.AddMonths(-1) },
            new Computer { Id = 4, Name = "PC-004", CreatedDate = DateTime.Now.AddMonths(-4) },
            new Computer { Id = 5, Name = "PC-005", CreatedDate = DateTime.Now.AddMonths(-10) },
            new Computer { Id = 6, Name = "PC-006", CreatedDate = DateTime.Now.AddMonths(-12) }
        };

        public IActionResult All(string searchString, string statusFilter, string sort, int page = 1, int pageSize = 5)
        {
            var computers = _computers.AsEnumerable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
                computers = computers.Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        

            // Sıralama
            computers = sort switch
            {
                "date_asc" => computers.OrderBy(c => c.CreatedDate),
                "date_desc" => computers.OrderByDescending(c => c.CreatedDate),
                _ => computers.OrderBy(c => c.Id)
            };

            // Sayfalama
            int totalCount = computers.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            computers = computers.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(computers.ToList());
        }

        public IActionResult Detail(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer == null)
                return NotFound();

            return View(computer);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Computer computer)
        {
            computer.Id = _computers.Max(c => c.Id) + 1;
            computer.CreatedDate = DateTime.Now;
            _computers.Add(computer);
            return RedirectToAction("All");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer == null)
                return NotFound();

            return View(computer);
        }

        [HttpPost]
        public IActionResult Edit(Computer updated)
        {
            var existing = _computers.FirstOrDefault(c => c.Id == updated.Id);
            if (existing == null)
                return NotFound();

            existing.Name = updated.Name;
           

            return RedirectToAction("All");
        }

        public IActionResult Delete(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer != null)
                _computers.Remove(computer);

            return RedirectToAction("All");
        }
    }
}
