using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Text;
using WebApplication1.EnvanterLib;

namespace WebApplication1.Controllers
{
    public class PoolController : Controller
    {
        private readonly AppDbContext _context;

        public PoolController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pool
        public async Task<IActionResult> Index(string search, string category, int? companyId, bool showOnlyAvailable = false)
        {
            try
            {
                // Base query - sadece havuzda olan ekipmanlar
                var query = _context.Computers
                    .Include(c => c.Company)
                    .Include(c => c.AssignedEmployee)
                    .AsQueryable();

                // Eğer showOnlyAvailable true ise sadece havuzda olanları göster
                if (showOnlyAvailable)
                {
                    query = query.Where(c => c.Status == ComputerStatus.Havuzda);
                }
                else
                {
                    // Aksi halde havuz ile ilgili tüm durumları göster (zimmetli hariç)
                    query = query.Where(c => c.Status == ComputerStatus.Havuzda ||
                                           c.Status == ComputerStatus.Bakim ||
                                           c.Status == ComputerStatus.Arızalı ||
                                           c.Status == ComputerStatus.Bozuk);
                }

                // Arama filtresi
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c => c.Name.Contains(search) ||
                                           c.AssetTag.Contains(search) ||
                                           c.SerialNumber.Contains(search) ||
                                           (c.Brand != null && c.Brand.Contains(search)) ||
                                           (c.Model != null && c.Model.Contains(search)));
                }

                // Kategori filtresi (şu an sadece Computer var, gelecekte genişletilebilir)
                if (!string.IsNullOrEmpty(category) && category != "Computer")
                {
                    // Gelecekte farklı kategoriler için filtreleme eklenebilir
                    query = query.Where(c => false); // Şimdilik boş sonuç döndür
                }

                // Firma filtresi
                if (companyId.HasValue)
                {
                    query = query.Where(c => c.CompanyId == companyId.Value);
                }

                // Sıralama - önce havuzda olanlar, sonra tarih
                query = query.OrderByDescending(c => c.Status == ComputerStatus.Havuzda)
                           .ThenByDescending(c => c.CreatedDate);

                var computers = await query.ToListAsync();

                // ViewBag verileri
                ViewBag.Search = search;
                ViewBag.Category = category;
                ViewBag.CompanyId = companyId?.ToString();
                ViewBag.ShowOnlyAvailable = showOnlyAvailable;
                ViewBag.Companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(computers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Havuz envanteri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Computer>());
            }
        }

        // GET: Pool/AssignMultiple
        public async Task<IActionResult> AssignMultiple(string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    TempData["Error"] = "Zimmetlenecek ekipman seçilmedi.";
                    return RedirectToAction(nameof(Index));
                }

                var computerIds = ids.Split(',').Select(int.Parse).ToList();
                var computers = await _context.Computers
                    .Include(c => c.Company)
                    .Where(c => computerIds.Contains(c.Id) && c.Status == ComputerStatus.Havuzda)
                    .ToListAsync();

                if (!computers.Any())
                {
                    TempData["Error"] = "Zimmetlenebilir ekipman bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Aktif çalışanları getir
                var employees = await _context.Employees
                    .Include(e => e.Company)
                    .Where(e => e.IsActive)
                    .OrderBy(e => e.Company!.Name)
                    .ThenBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();

                ViewBag.Employees = employees;
                ViewBag.SelectedComputerIds = computerIds;

                return View(computers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Zimmetleme sayfası yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pool/AssignMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMultiple(List<int> computerIds, int employeeId, DateTime assignmentDate, string? notes)
        {
            try
            {
                if (!computerIds.Any())
                {
                    TempData["Error"] = "Zimmetlenecek ekipman seçilmedi.";
                    return RedirectToAction(nameof(Index));
                }

                var employee = await _context.Employees
                    .Include(e => e.Company)
                    .FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive);

                if (employee == null)
                {
                    TempData["Error"] = "Seçilen çalışan bulunamadı veya aktif değil.";
                    return RedirectToAction(nameof(Index));
                }

                var computers = await _context.Computers
                    .Where(c => computerIds.Contains(c.Id) && c.Status == ComputerStatus.Havuzda)
                    .ToListAsync();

                if (!computers.Any())
                {
                    TempData["Error"] = "Zimmetlenebilir ekipman bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Zimmetleme işlemi
                foreach (var computer in computers)
                {
                    computer.AssignedEmployeeId = employeeId;
                    computer.Status = ComputerStatus.Zimmetli;
                    computer.LastUpdatedDate = DateTime.UtcNow;
                    computer.LastUpdatedBy = this.GetUserFromHttpContext()!.Email ?? "Unknown";
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"{computers.Count} adet ekipman {employee.FirstName} {employee.LastName} ({employee.Company?.Name}) adlı çalışana başarıyla zimmetlendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Zimmetleme işlemi sırasında hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

       
        
        

        // Helper method for status display
        private string GetStatusDisplayName(ComputerStatus status)
        {
            return status switch
            {
                ComputerStatus.Havuzda => "Havuzda",
                ComputerStatus.Zimmetli => "Zimmetli",
                ComputerStatus.Kullanımda => "Kullanımda",
                ComputerStatus.Bakim => "Bakımda",
                ComputerStatus.Arızalı => "Arızalı",
                ComputerStatus.Aktif => "Aktif",
                ComputerStatus.Bozuk => "Bozuk",
                _ => status.ToString()
            };
        }
    }
}
