
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class ComputerController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly int _pageSize = 10;

        public async Task<IActionResult> Index(string searchString, string statusFilter,
            string sortOrder, int page = 1)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = page;

            var computers = _context.Computers.AsQueryable();

            // Arama
            if (!string.IsNullOrEmpty(searchString))
            {
                computers = computers.Where(c =>
                    (c.Name != null && c.Name.Contains(searchString)) ||
                    (c.AssetTag != null && c.AssetTag.Contains(searchString)) ||
                    (c.SerialNumber != null && c.SerialNumber.Contains(searchString)) ||
                    (c.Brand != null && c.Brand.Contains(searchString)) ||
                    (c.Model != null && c.Model.Contains(searchString)));
            }

            // Durum filtreleme
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<ComputerStatus>(statusFilter, true, out var status))
                {
                    computers = computers.Where(c => c.Status == status);
                }
            }

            // Sıralama
            computers = sortOrder switch
            {
                "name_desc" => computers.OrderByDescending(c => c.Name),
                "name_asc" => computers.OrderBy(c => c.Name),
                "date_desc" => computers.OrderByDescending(c => c.CreatedDate),
                "date_asc" => computers.OrderBy(c => c.CreatedDate),
                "status_desc" => computers.OrderByDescending(c => c.Status),
                "status_asc" => computers.OrderBy(c => c.Status),
                "brand_desc" => computers.OrderByDescending(c => c.Brand),
                "brand_asc" => computers.OrderBy(c => c.Brand),
                _ => computers.OrderByDescending(c => c.CreatedDate)
            };

            // Sayfalama
            var totalCount = await computers.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)_pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            var computersForPage = await computers
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Dashboard istatistikleri
            var stats = new
            {
                Total = await _context.Computers.CountAsync(),
                Active = await _context.Computers.Where(c => c.Status == ComputerStatus.Aktif).CountAsync(),
                InMaintenance = await _context.Computers.Where(c => c.Status == ComputerStatus.Bakim).CountAsync(),
                Broken = await _context.Computers.Where(c => c.Status == ComputerStatus.Bozuk).CountAsync(),
                UnderWarranty = await _context.Computers.Where(c => c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value > DateTime.Now).CountAsync()
            };

            ViewBag.Stats = stats;

            return View(computersForPage);
        }

        public IActionResult Create()
        {
            ViewBag.StatusList = GetStatusSelectList();
            ViewBag.StorageTypeList = GetStorageTypeSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Computer model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.CreatedDate = DateTime.Now;
                    model.LastUpdatedDate = DateTime.Now;
                    model.LastUpdatedBy = User.Identity?.Name ?? "System";

                    _context.Computers.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Bilgisayar başarıyla eklendi.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            }

            ViewBag.StatusList = GetStatusSelectList();
            ViewBag.StorageTypeList = GetStorageTypeSelectList();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
                return NotFound();

            ViewBag.StatusList = GetStatusSelectList();
            ViewBag.StorageTypeList = GetStorageTypeSelectList();
            return View(computer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Computer comp)
        {
            if (id != comp.Id)
                return NotFound();

            try
            {
                if (ModelState.IsValid)
                {
                    comp.LastUpdatedDate = DateTime.Now;
                    comp.LastUpdatedBy = User.Identity?.Name ?? "System";

                    _context.Update(comp);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Bilgisayar başarıyla güncellendi.";
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ComputerExists(comp.Id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            }

            ViewBag.StatusList = GetStatusSelectList();
            ViewBag.StorageTypeList = GetStorageTypeSelectList();
            return View(comp);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var computer = await _context.Computers.FindAsync(id);
            if (computer == null)
                return NotFound();

            return View(computer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var computer = await _context.Computers.FindAsync(id);
                if (computer != null)
                {
                    _context.Computers.Remove(computer);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Bilgisayar başarıyla silindi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Silme işlemi sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var computer = await _context.Computers.FirstOrDefaultAsync(c => c.Id == id);
            if (computer == null)
                return NotFound();

            return View(computer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ComputerStatus status)
        {
            try
            {
                var computer = await _context.Computers.FindAsync(id);
                if (computer == null)
                    return Json(new { success = false, message = "Bilgisayar bulunamadı." });

                computer.Status = status;
                computer.LastUpdatedDate = DateTime.Now;
                computer.LastUpdatedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Durum başarıyla güncellendi.",
                    statusName = computer.StatusDisplayName,
                    statusClass = computer.StatusBadgeClass
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateStatus(List<int> computerIds, ComputerStatus status)
        {
            try
            {
                var computers = await _context.Computers
                    .Where(c => computerIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var computer in computers)
                {
                    computer.Status = status;
                    computer.LastUpdatedDate = DateTime.Now;
                    computer.LastUpdatedBy = User.Identity?.Name ?? "System";
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"{computers.Count} bilgisayarın durumu güncellendi."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Export()
        {
            var computers = await _context.Computers.ToListAsync();

            // CSV export logic burada implement edilebilir
            // Şu an için sadece view döndürüyoruz
            return View(computers);
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalComputers = await _context.Computers.CountAsync(),
                ActiveComputers = await _context.Computers.Where(c => c.Status == ComputerStatus.Aktif).CountAsync(),
                InMaintenanceComputers = await _context.Computers.Where(c => c.Status == ComputerStatus.Bakim).CountAsync(),
                BrokenComputers = await _context.Computers.Where(c => c.Status == ComputerStatus.Bozuk).CountAsync(),
                UnderWarrantyComputers = await _context.Computers.Where(c => c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value > DateTime.Now).CountAsync(),
                ExpiredWarrantyComputers = await _context.Computers.Where(c => c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value <= DateTime.Now).CountAsync(),
                RecentlyAddedComputers = await _context.Computers.Where(c => c.CreatedDate >= DateTime.Now.AddDays(-30)).CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        private async Task<bool> ComputerExists(int id)
        {
            return await _context.Computers.AnyAsync(e => e.Id == id);
        }

        private static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetStatusSelectList()
        {
            return [.. Enum.GetValues<ComputerStatus>()
                .Cast<ComputerStatus>()
                .Select(status => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = status.ToString().Replace("_", " ")
                })];
        }

        private static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetStorageTypeSelectList()
        {
            return [.. Enum.GetValues<StorageType>()
                .Cast<StorageType>()
                .Select(type => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = ((int)type).ToString(),
                    Text = type.ToString()
                })];
        }
    }
}