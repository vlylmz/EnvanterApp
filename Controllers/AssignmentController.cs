using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class AssignmentController : Controller
    {
        // Geçici veri kaynakları (PoolController'dakilerle aynı, dışarıdan erişilecekse ortak bir servise taşınmalı)
        private static List<Computer> _computers = PoolController.GetComputers();
        private static List<Employee> _employees = PoolController.GetEmployees();

        // Zimmet listesi
        public IActionResult Index()
        {
            var assigned = _computers
                .Where(c => c.Status == "InUse" && c.AssignedEmployeeId.HasValue)
                .Select(c =>
                {
                    var emp = _employees.FirstOrDefault(e => e.Id == c.AssignedEmployeeId);
                    return new ComputerDetailsViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AssetTag = c.AssetTag,
                        Status = c.Status,
                        PurchaseDate = c.PurchaseDate,
                        AssignedUserFullName = emp?.FirstName,
                        AssignedUserEmail = emp?.Email ?? "Yok"
                    };
                }).ToList();

            return View(assigned);
        }

        // Zimmetleme formu
        public IActionResult Create()
        {
            ViewBag.Computers = _computers.Where(c => c.Status != "InUse").ToList();
            ViewBag.Employees = _employees;
            return View();
        }

        [HttpPost]
        public IActionResult Create(int computerId, int employeeId)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == computerId);
            var employee = _employees.FirstOrDefault(e => e.Id == employeeId);

            if (computer != null && employee != null)
            {
                computer.Status = "InUse";
                computer.AssignedEmployeeId = employee.Id;
            }

            return RedirectToAction("Index");
        }

        // İade işlemi
        public IActionResult Unassign(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer != null)
            {
                computer.Status = "InPool";
                computer.AssignedEmployeeId = null;
            }

            return RedirectToAction("Index");
        }
    }
}
