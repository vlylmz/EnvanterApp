using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class EmployeeController : Controller
    {
        // Geçici in-memory örnek data
        private static List<Employee> _employees = new List<Employee>
        {
            new Employee { Id = 1, FirstName = "Ali", LastName = "Yılmaz", Email = "ali@firma.com", IsActive = true, CompanyId = 1 },
            new Employee { Id = 2, FirstName = "Ayşe", LastName = "Demir", Email = "ayse@firma.com", IsActive = true, CompanyId = 2 },
            new Employee { Id = 3, FirstName = "Murat", LastName = "Kaya", Email = "murat@firma.com", IsActive = false, CompanyId = 1 }
        };

        public IActionResult List(string searchString, string statusFilter)
        {
            var employees = _employees.AsQueryable();

            // Arama filtresi (isim veya soyisimde geçenler)
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e =>
                    (e.FirstName + " " + e.LastName).ToLower().Contains(searchString.ToLower()) ||
                    (e.Email != null && e.Email.ToLower().Contains(searchString.ToLower()) )
                );
            }

            // Durum filtresi
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "aktif")
                    employees = employees.Where(e => e.IsActive);
                else if (statusFilter == "pasif")
                    employees = employees.Where(e => !e.IsActive);
            }

            // Filtrelenen değerleri tekrar view'a taşı
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;

            return View(employees.ToList());
        }

        [HttpGet]

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            employee.Id = _employees.Max(e => e.Id) + 1;
            _employees.Add(employee);
            return RedirectToAction("List");
        }
        // Detay
        public IActionResult Detail(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // Düzenle
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }
        [HttpPost]
        public IActionResult Edit(Employee updated)
        {
            var emp = _employees.FirstOrDefault(e => e.Id == updated.Id);
            if (emp == null) return NotFound();
            emp.FirstName = updated.FirstName;
            emp.LastName = updated.LastName;
            emp.Email = updated.Email;
            emp.IsActive = updated.IsActive;
            emp.CompanyId = updated.CompanyId;
            return RedirectToAction("List");
        }

        // Sil
        public IActionResult Delete(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee != null)
                _employees.Remove(employee);
            return RedirectToAction("List");
        }
        

    }
}
