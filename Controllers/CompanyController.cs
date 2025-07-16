using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using System.Collections.Generic;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class CompanyController : Controller
    {
        private static List<Company> _companies = new List<Company>
{              
            // Geçici örnek veriler
            new Company { Id = 1, Name = "Spiltech", Description = "Teknoloji firması", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-2) },
            new Company { Id = 2, Name = "MBB", Description = "Belediye iştiraki", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-6) },
            new Company { Id = 3, Name = "Manisa Enerji", Description = "Enerji firması", IsActive = false, CreatedDate = DateTime.Now.AddYears(-1) },
            new Company { Id = 4, Name = "Spilaş", Description = "Grup şirketi", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-8) },
            new Company { Id = 5, Name = "NetCore Yazılım", Description = "Yazılım geliştirme", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-5) },
            new Company { Id = 6, Name = "Okyanus Bilgi", Description = "IT danışmanlık", IsActive = false, CreatedDate = DateTime.Now.AddMonths(-10) },
            new Company { Id = 7, Name = "Ege Sistem", Description = "Donanım tedarik", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-3) },
            new Company { Id = 8, Name = "Delta Elektrik", Description = "Elektrik çözümleri", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-7) },
            new Company { Id = 9, Name = "Mega Bilişim", Description = "Bilgi teknolojileri", IsActive = false, CreatedDate = DateTime.Now.AddYears(-2) },
            new Company { Id = 10, Name = "Vizyon Medya", Description = "Medya hizmetleri", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-12) },
            new Company { Id = 11, Name = "Atlas Data", Description = "Veri merkezi yönetimi", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-1) },
            new Company { Id = 12, Name = "Alfa Arge", Description = "Ar-Ge firması", IsActive = false, CreatedDate = DateTime.Now.AddMonths(-14) },
            new Company { Id = 13, Name = "Birlik Network", Description = "Ağ çözümleri", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-9) },
            new Company { Id = 14, Name = "Penta Yazılım", Description = "Kurumsal yazılım", IsActive = true, CreatedDate = DateTime.Now.AddMonths(-4) },
            new Company { Id = 15, Name = "Beta Ofis", Description = "Ofis destek hizmetleri", IsActive = false, CreatedDate = DateTime.Now.AddMonths(-15) }
};


        public IActionResult All(string searchString, string statusFilter, string sort, int page = 1, int pageSize = 10)
        {
            var companies = _companies.AsEnumerable();

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
                companies = companies.Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));

            // Durum filtresi
            if (!string.IsNullOrEmpty(statusFilter))
                companies = companies.Where(c => c.IsActive == (statusFilter == "aktif"));

            // Sıralama
            companies = sort switch
            {
                "date_asc" => companies.OrderBy(c => c.CreatedDate),
                "date_desc" => companies.OrderByDescending(c => c.CreatedDate),
                _ => companies.OrderBy(c => c.Id)
            };

            // Sayfalama için toplam sayfa sayısı ve mevcut sayfa listesi
            int totalCount = companies.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            companies = companies.Skip((page - 1) * pageSize).Take(pageSize);

            // ViewBag ile sayfalama bilgisi de gönder
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentSort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(companies.ToList());
        }



        // Geçici örnek veriler kullanıldı
        public IActionResult Detail(int id)
        {
            var company = _companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
                return NotFound();

            // Örnek veriler - ileride veritabanına bağlayınca dinamik olur!
            var employees = new List<Employee>
    {
        new Employee { Id = 1, FirstName = "Ali", LastName = "Yılmaz", Email = "ali@spiltech.com", IsActive = true },
        new Employee { Id = 2, FirstName = "Ayşe", LastName = "Demir", Email = "ayse@spiltech.com", IsActive = true },
        new Employee { Id = 3, FirstName = "Murat", LastName = "Kaya", Email = "murat@spiltech.com", IsActive = false }
    };

            var computers = new List<Computer>
    {
        new Computer { Id = 1, Name = "PC-001", AssetTag = "A123", Status = "Kullanımda" },
        new Computer { Id = 2, Name = "PC-002", AssetTag = "A124", Status = "Havuzda" }
    };

            var softwares = new List<Software>
    {
        new Software { Id = 1, Name = "Windows 11 Pro", Brand = "Microsoft", Status = "Aktif" },
        new Software { Id = 2, Name = "Office 365", Brand = "Microsoft", Status = "Aktif" }
    };

            var supplies = new List<Supply>
    {
        new Supply { Id = 1, Name = "Toner", SystemBarcode = "TNR-001", Status = "Depoda" },
        new Supply { Id = 2, Name = "Klavye", SystemBarcode = "KYB-001", Status = "Kullanımda" }
    };

            var viewModel = new CompanyDetailsViewModel
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                IsActive = company.IsActive,
                CreatedDate = company.CreatedDate,
                EmployeeCount = employees.Count,
                ComputerCount = computers.Count,
                SoftwareCount = softwares.Count,
                SupplyCount = supplies.Count,
                Employees = employees,
                Computers = computers,
                Softwares = softwares,
                Supplies = supplies
            };

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Company company)
        {
            company.Id = _companies.Max(c => c.Id) + 1;
            company.CreatedDate = DateTime.Now;
            _companies.Add(company);
            return RedirectToAction("All");
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var company = _companies.FirstOrDefault(c => c.Id == id);
            if (company == null)
                return NotFound();

            return View(company);
        }

        [HttpPost]
        public IActionResult Edit(Company updatedCompany)
        {
            var existing = _companies.FirstOrDefault(c => c.Id == updatedCompany.Id);
            if (existing == null)
                return NotFound();

            existing.Name = updatedCompany.Name;
            existing.Description = updatedCompany.Description;
            existing.IsActive = updatedCompany.IsActive;

            return RedirectToAction("All");
        }
        public IActionResult Delete(int id)
        {
            var company = _companies.FirstOrDefault(c => c.Id == id);
            if (company != null)
            {
                _companies.Remove(company);
            }

            return RedirectToAction("All");
        }   


    }
}
