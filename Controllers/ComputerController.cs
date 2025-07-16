using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.ViewModels;
=======
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;
>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b

namespace WebApplication1.Controllers
{
    public class ComputerController : Controller
    {
        private static List<Computer> _computers = new List<Computer>
        {
<<<<<<< HEAD
            new Computer { Id = 1, Name = "PC-001", Description = "Muhasebe bilgisayarı", IsActive = true},
            new Computer { Id = 2, Name = "PC-002", Description = "Toplantı odası bilgisayarı", IsActive = false},
            new Computer { Id = 3, Name = "PC-003", Description = "Genel kullanım", IsActive = true},
            new Computer { Id = 4, Name = "PC-004", Description = "Yönetici bilgisayarı", IsActive = true},
            new Computer { Id = 5, Name = "PC-005", Description = "Yedek cihaz", IsActive = false},
            new Computer { Id = 6, Name = "PC-006", Description = "Sunucu makinesi", IsActive = true }
=======
            new Computer { Id = 1, Name = "PC-001", CreatedDate = DateTime.Now.AddMonths(-6) },
            new Computer { Id = 2, Name = "PC-002", CreatedDate = DateTime.Now.AddMonths(-2) },
            new Computer { Id = 3, Name = "PC-003", CreatedDate = DateTime.Now.AddMonths(-1) },
            new Computer { Id = 4, Name = "PC-004", CreatedDate = DateTime.Now.AddMonths(-4) },
            new Computer { Id = 5, Name = "PC-005", CreatedDate = DateTime.Now.AddMonths(-10) },
            new Computer { Id = 6, Name = "PC-006", CreatedDate = DateTime.Now.AddMonths(-12) }
>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
        };

        public IActionResult All(string searchString, string statusFilter, string sort, int page = 1, int pageSize = 5)
        {
            var computers = _computers.AsEnumerable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
                computers = computers.Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

<<<<<<< HEAD
            // Durum filtresi
            if (!string.IsNullOrEmpty(statusFilter))
                computers = computers.Where(c => c.IsActive == (statusFilter == "aktif"));
=======
        
>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b

            // Sıralama
            computers = sort switch
            {
<<<<<<< HEAD
                "date_asc" => computers.OrderBy(c => c.PurchaseDate),
                "date_desc" => computers.OrderByDescending(c => c.PurchaseDate),
=======
                "date_asc" => computers.OrderBy(c => c.CreatedDate),
                "date_desc" => computers.OrderByDescending(c => c.CreatedDate),
>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
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
<<<<<<< HEAD
=======

>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
        public IActionResult Detail(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer == null)
                return NotFound();

<<<<<<< HEAD
            // Sahip (örnek kullanıcı)
            var assignedUser = new Employee
            {
                FirstName = "Ali",
                LastName = "Yılmaz",
                Email = "ali@example.com",
                IsActive = true
            };

            // Yüklü yazılımlar
            var installedSoftwares = new List<Software>
    {
        new Software { Id = 1, Name = "Windows 11", Brand = "Microsoft", Status = "Lisanslı" },
        new Software { Id = 2, Name = "Chrome", Brand = "Google", Status = "Ücretsiz" }
    };

            // Aksesuarlar
            var accessories = new List<Supply>
    {
        new Supply { Id = 1, Name = "Klavye", SystemBarcode = "KYB-001", Status = "Takılı" },
        new Supply { Id = 2, Name = "Mouse", SystemBarcode = "MSE-001", Status = "Takılı" }
    };

            var viewModel = new ComputerDetailsViewModel
            {
                Id = computer.Id,
                Name = computer.Name,
                Description = computer.Description,
                AssetTag = computer.AssetTag,
                Status = computer.Status,
                IsActive = computer.IsActive,
                CreatedDate = computer.PurchaseDate,
                AssignedUserFullName = $"{assignedUser.FirstName} {assignedUser.LastName}",
                AssignedUserEmail = assignedUser.Email,
                InstalledSoftwares = installedSoftwares,
                Accessories = accessories
            };

            return View(viewModel);
        }




=======
            return View(computer);
        }

>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
<<<<<<< HEAD
        public IActionResult Create(ComputerDetailsViewModel model)
        {
            var newComputer = new Computer
            {
                Id = _computers.Max(c => c.Id) + 1,
                Name = model.Name,
                Description = model.Description,
                AssetTag = model.AssetTag,
                Status = model.Status,
                IsActive = model.IsActive,
            };

            _computers.Add(newComputer);
            return RedirectToAction("All");
        }


=======
        public IActionResult Create(Computer computer)
        {
            computer.Id = _computers.Max(c => c.Id) + 1;
            computer.CreatedDate = DateTime.Now;
            _computers.Add(computer);
            return RedirectToAction("All");
        }

>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer == null)
                return NotFound();

<<<<<<< HEAD
            var model = new ComputerDetailsViewModel
            {
                Id = computer.Id,
                Name = computer.Name,
                Description = computer.Description,
                AssetTag = computer.AssetTag,
                Status = computer.Status,
                IsActive = computer.IsActive,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ComputerDetailsViewModel model)
        {
            var existing = _computers.FirstOrDefault(c => c.Id == model.Id);
            if (existing == null)
                return NotFound();

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.AssetTag = model.AssetTag;
            existing.Status = model.Status;
            existing.IsActive = model.IsActive;
=======
            return View(computer);
        }

        [HttpPost]
        public IActionResult Edit(Computer updated)
        {
            var existing = _computers.FirstOrDefault(c => c.Id == updated.Id);
            if (existing == null)
                return NotFound();

            existing.Name = updated.Name;
           
>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b

            return RedirectToAction("All");
        }

<<<<<<< HEAD

=======
>>>>>>> e32743f30c5290420fe408ef72af005dc8e0646b
        public IActionResult Delete(int id)
        {
            var computer = _computers.FirstOrDefault(c => c.Id == id);
            if (computer != null)
                _computers.Remove(computer);

            return RedirectToAction("All");
        }
    }
}
