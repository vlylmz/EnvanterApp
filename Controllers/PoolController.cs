using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class PoolController : Controller
    {
        // Geçici veri kaynağı  
        private static List<Company> _companies = new List<Company>
            {
                new Company { Id = 1, Name = "Spilaş" },
                new Company { Id = 2, Name = "Manisa Enerji" }  
                // ...  
            };

        private static List<Computer> _computers = new List<Computer>
            {
                new Computer { Id = 1, Name = "HP Elitebook", CompanyId = 1, Status ="InPool", SerialNumber = "HP123", AssetTag = "IT-1001", IsActive = true },
                new Computer { Id = 2, Name = "Dell Inspiron", CompanyId = 2, Status = "InPool", SerialNumber = "DL456", AssetTag = "IT-1002", IsActive = true },
                new Computer { Id = 3, Name = "Lenovo ThinkPad", CompanyId = 1, Status = "InUse", AssignedEmployeeId = 5, SerialNumber = "LV789", AssetTag = "IT-1003", IsActive = true },  
                // ...  
            };

        // Çalışanlar için geçici veri kaynağı  
        private static List<Employee> _employees = new List<Employee>
            {
                new Employee { Id = 1, FirstName = "Ali", CompanyId = 1 },
                new Employee { Id = 2, FirstName = "Ayşe", CompanyId = 2 },
                new Employee { Id = 3, FirstName = "Mehmet", CompanyId = 1 },  
                // ...  
            };
        private object _context;

        public IActionResult Index(int? companyId)
        {
            // Sadece havuzda olanlar  
            var poolItems = _computers
                .Where(x => x.Status == "InPool")
                .ToList();

            // Firma filtresi uygula  
            if (companyId.HasValue)
                poolItems = poolItems.Where(x => x.CompanyId == companyId.Value).ToList();

            // Company bilgisini doldur  
            foreach (var c in poolItems)
                c.Company = _companies.FirstOrDefault(co => co.Id == c.CompanyId);

            ViewBag.Companies = _companies;
            ViewBag.SelectedCompany = companyId;

            return View(poolItems);
        }

        public IActionResult SetInUse(int id)
        {
            var pc = _computers.FirstOrDefault(x => x.Id == id);
            if (pc != null)
            {
                pc.Status = "InUse";
                // AssignedEmployeeId atanabilir (isteğe göre)  
            }
            return RedirectToAction("Index");
        }

        // Zimmetleme sayfası  
        public IActionResult Assign(int id)
        {
            var computer = _computers.FirstOrDefault(x => x.Id == id);
            if (computer == null) return NotFound();

            // Sadece aynı şirketteki çalışanlar  
            var employees = _employees.Where(e => e.CompanyId == computer.CompanyId).ToList();
            ViewBag.Employees = employees;
            return View(computer);
        }

        [HttpPost]
        public IActionResult Assign(int id, int employeeId)
        {
            var computer = _computers.FirstOrDefault(x => x.Id == id);
            var employee = _employees.FirstOrDefault(e => e.Id == employeeId);

            if (computer != null && employee != null && computer.CompanyId == employee.CompanyId)
            {
                computer.Status = "InUse";
                computer.AssignedEmployeeId = employee.Id;
            }
            return RedirectToAction("Index");
        }
        // Yeni ürün ekleme formu
        public IActionResult Create()
        {
            ViewBag.Companies = _companies; // Firma seçimi için
            return View();
        }

        // Ekleme işlemi (Form POST)
        [HttpPost]
        public IActionResult Create(Computer model)
        {
            // Basit id üretici (id tekrarı olmaması için)
            int newId = _computers.Count > 0 ? _computers.Max(x => x.Id) + 1 : 1;
            model.Id = newId;

            // İlk eklemede varsayılan olarak havuzda olsun
            model.Status = "InPool";

            // İlgili şirkete ait olduğu için
            var company = _companies.FirstOrDefault(c => c.Id == model.CompanyId);
            if (company == null)
            {
                ModelState.AddModelError("CompanyId", "Geçerli bir şirket seçmelisiniz.");
                ViewBag.Companies = _companies;
                return View(model);
            }

            _computers.Add(model);

            return RedirectToAction("Index");
        }
        public IActionResult Detail(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer == null)
                return NotFound();

            var viewModel = new ComputerDetailsViewModel
            {
                Id = computer.Id,
                Name = computer.Name,
                Description = computer.Description,
                AssetTag = computer.AssetTag,
                Status = computer.Status,
                IsActive = computer.IsActive,
                PurchaseDate = computer.PurchaseDate
            };

            return View(viewModel);
        }

        public static List<Computer> GetComputers()
        {
            // Örnek veri döndürmek için bir liste oluşturabilirsiniz.
            return new List<Computer>
            {
                new Computer { Id = 1, Name = "Computer1", Status = "InPool", AssetTag = "CT001", PurchaseDate = DateTime.Now },
                new Computer { Id = 2, Name = "Computer2", Status = "InUse", AssetTag = "CT002", PurchaseDate = DateTime.Now, AssignedEmployeeId = 1 }
            };
        }

        public static List<Employee> GetEmployees()
        {
            // Örnek veri döndürmek için bir liste oluşturabilirsiniz.
            return new List<Employee>
            {
                new Employee { Id = 1, FirstName = "John", Email = "john@example.com" },
                new Employee { Id = 2, FirstName = "Jane", Email = "jane@example.com" }
            };
        }
    }

}

