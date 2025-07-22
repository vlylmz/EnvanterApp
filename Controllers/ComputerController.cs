using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    public class ComputerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly int _pageSize = 10;

        public ComputerController(AppDbContext context)
        {
            _context = context;
        }

        // Ana Index metodu - hem normal hem AJAX istekleri destekler
        public async Task<IActionResult> Index(string? searchString, string? statusFilter,
            string? companyFilter, string? sortOrder, int page = 1, bool isAjax = false)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SearchString = searchString;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CompanyFilter = companyFilter;
            ViewBag.CurrentPage = page;

            var result = await GetFilteredComputersAsync(searchString, statusFilter, companyFilter, sortOrder, page);

            // AJAX isteği ise sadece data döndür
            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    data = result.Computers.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name ?? "",
                        assetTag = c.AssetTag ?? "",
                        serialNumber = c.SerialNumber ?? "",
                        brand = c.Brand ?? "",
                        model = c.Model ?? "",
                        company = c.Company?.Name ?? "",
                        assignedEmployee = c.AssignedEmployee != null
                            ? $"{c.AssignedEmployee.FirstName} {c.AssignedEmployee.LastName}"
                            : null,
                        status = c.Status.ToString(),
                        statusDisplayName = c.StatusDisplayName ?? "",
                        statusBadgeClass = c.StatusBadgeClass ?? "",
                        processorName = c.ProcessorName ?? "",
                        ramAmount = c.RamAmount,
                        ramType = c.RamType ?? "",
                        storageSize = c.StorageSize,
                        storageType = c.StorageType?.ToString() ?? "",
                        warrantyEndDate = c.WarrantyEndDate?.ToString("dd.MM.yyyy"),
                        warrantyDaysRemaining = c.WarrantyDaysRemaining,
                        createdDate = c.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                    }),
                    pagination = new
                    {
                        currentPage = page,
                        totalPages = result.TotalPages,
                        totalCount = result.TotalCount,
                        pageSize = _pageSize
                    },
                    stats = result.Stats
                });
            }

            // Normal sayfa yüklemesi
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalCount;
            ViewBag.Stats = result.Stats;

            await LoadFilterData();
            return View(result.Computers);
        }

        // Yeni AJAX endpoint - sadece filtrelenmiş data döndürür
        [HttpGet]
        public async Task<JsonResult> GetFilteredData(string? searchString, string? statusFilter,
            string? companyFilter, string? sortOrder, int page = 1)
        {
            try
            {
                var result = await GetFilteredComputersAsync(searchString, statusFilter, companyFilter, sortOrder, page);

                return Json(new
                {
                    success = true,
                    data = result.Computers.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name ?? "",
                        assetTag = c.AssetTag ?? "",
                        serialNumber = c.SerialNumber ?? "",
                        brand = c.Brand ?? "",
                        model = c.Model ?? "",
                        company = c.Company != null ? new
                        {
                            id = c.Company.Id,
                            name = c.Company.Name ?? ""
                        } : null,
                        assignedEmployee = c.AssignedEmployee != null ? new
                        {
                            id = c.AssignedEmployee.Id,
                            name = $"{c.AssignedEmployee.FirstName} {c.AssignedEmployee.LastName}",
                            firstName = c.AssignedEmployee.FirstName ?? ""
                        } : null,
                        status = new
                        {
                            value = (int)c.Status,
                            name = c.Status.ToString(),
                            displayName = c.StatusDisplayName ?? "",
                            badgeClass = c.StatusBadgeClass ?? ""
                        },
                        hardware = new
                        {
                            processor = c.ProcessorName ?? "",
                            ram = new
                            {
                                amount = c.RamAmount,
                                type = c.RamType ?? ""
                            },
                            storage = new
                            {
                                size = c.StorageSize,
                                type = c.StorageType?.ToString() ?? ""
                            }
                        },
                        warranty = new
                        {
                            endDate = c.WarrantyEndDate?.ToString("dd.MM.yyyy"),
                            daysRemaining = c.WarrantyDaysRemaining,
                            isExpired = c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value <= DateTime.Now
                        },
                        dates = new
                        {
                            created = c.CreatedDate.ToString("dd.MM.yyyy HH:mm"),
                            lastUpdated = c.LastUpdatedDate?.ToString("dd.MM.yyyy HH:mm")
                        }
                    }),
                    pagination = new
                    {
                        currentPage = page,
                        totalPages = result.TotalPages,
                        totalCount = result.TotalCount,
                        pageSize = _pageSize,
                        hasNextPage = page < result.TotalPages,
                        hasPreviousPage = page > 1
                    },
                    stats = result.Stats,
                    filters = new
                    {
                        searchString,
                        statusFilter,
                        companyFilter,
                        sortOrder
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // İstatistikleri gerçek zamanlı olarak döndüren endpoint
        [HttpGet]
        public async Task<JsonResult> GetDashboardStats()
        {
            try
            {
                var stats = await CalculateStatsAsync();

                return Json(new
                {
                    success = true,
                    stats = stats,
                    lastUpdated = DateTime.Now.ToString("HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Hızlı arama endpoint'i - autocomplete için
        [HttpGet]
        public async Task<JsonResult> QuickSearch(string? term, int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new { success = true, suggestions = new List<object>() });
                }

                var suggestions = await _context.Computers
                    .Include(c => c.Company)
                    .Include(c => c.AssignedEmployee)
                    .Where(c =>
                        (c.Name != null && c.Name.Contains(term)) ||
                        (c.AssetTag != null && c.AssetTag.Contains(term)) ||
                        (c.SerialNumber != null && c.SerialNumber.Contains(term)) ||
                        (c.Brand != null && c.Brand.Contains(term)) ||
                        (c.Model != null && c.Model.Contains(term)) ||
                        (c.Company != null && c.Company.Name.Contains(term)) ||
                        (c.AssignedEmployee != null &&
                         (c.AssignedEmployee.FirstName + " " + c.AssignedEmployee.LastName).Contains(term)))
                    .Take(limit)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name ?? "",
                        assetTag = c.AssetTag ?? "",
                        company = c.Company != null ? c.Company.Name : null,
                        type = "computer",
                        icon = "fas fa-desktop"
                    })
                    .ToListAsync();

                // Arama terimlerine göre kategorize et
                var categories = new List<object>();

                if (suggestions.Any())
                {
                    categories.Add(new
                    {
                        name = "Bilgisayarlar",
                        items = suggestions
                    });
                }

                // Eğer çok az sonuç varsa, company önerilerini de ekle
                if (suggestions.Count < 5)
                {
                    var companyMatches = await _context.Companies
                        .Where(c => c.Name.Contains(term))
                        .Take(3)
                        .Select(c => new
                        {
                            id = c.Id,
                            name = c.Name ?? "",
                            type = "company",
                            icon = "fas fa-building"
                        })
                        .ToListAsync();

                    if (companyMatches.Any())
                    {
                        categories.Add(new
                        {
                            name = "Firmalar",
                            items = companyMatches
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    categories = categories,
                    totalCount = suggestions.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Gelişmiş filtreleme seçenekleri endpoint'i
        [HttpGet]
        public async Task<JsonResult> GetFilterOptions()
        {
            try
            {
                // Enum değerlerini sync olarak al
                var statusOptions = Enum.GetValues(typeof(ComputerStatus))
                    .Cast<ComputerStatus>()
                    .Select(status => new
                    {
                        value = (int)status,
                        name = status.ToString(),
                        displayName = GetStatusDisplayName(status),
                        badgeClass = GetStatusBadgeClass(status),
                        count = _context.Computers.Count(c => c.Status == status)
                    })
                    .ToList();

                // Company verilerini al - eğer Computers navigation property yoksa farklı yaklaşım
                var companyOptions = await _context.Companies
                    .Select(c => new
                    {
                        value = c.Id,
                        name = c.Name ?? "",
                        count = _context.Computers.Count(comp => comp.CompanyId == c.Id)
                    })
                    .Where(c => c.count > 0)
                    .OrderByDescending(c => c.count)
                    .ToListAsync();

                var brandOptions = await _context.Computers
                    .Where(c => !string.IsNullOrEmpty(c.Brand))
                    .GroupBy(c => c.Brand)
                    .Select(g => new
                    {
                        value = g.Key ?? "",
                        name = g.Key ?? "",
                        count = g.Count()
                    })
                    .OrderByDescending(c => c.count)
                    .Take(10)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    options = new
                    {
                        statuses = statusOptions,
                        companies = companyOptions,
                        brands = brandOptions
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Bulk işlemler için geliştirilmiş endpoint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(string action, List<int> computerIds, object? data = null)
        {
            try
            {
                if (computerIds == null || !computerIds.Any())
                {
                    return Json(new { success = false, message = "Hiçbir bilgisayar seçilmedi." });
                }

                var computers = await _context.Computers
                    .Where(c => computerIds.Contains(c.Id))
                    .ToListAsync();

                if (!computers.Any())
                {
                    return Json(new { success = false, message = "Seçilen bilgisayarlar bulunamadı." });
                }

                dynamic result = action.ToLower() switch
                {
                    "updatestatus" => await BulkUpdateStatusInternal(computers, data),
                    "assignemployee" => await BulkAssignEmployeeInternal(computers, data),
                    "updatecompany" => await BulkUpdateCompanyInternal(computers, data),
                    "delete" => await BulkDeleteInternal(computers),
                    "export" => await BulkExportInternal(computers),
                    _ => new { success = false, message = "Geçersiz işlem." }
                };

                // Dynamic object'ten success özelliğini güvenli şekilde kontrol et
                var resultType = result.GetType();
                var successProperty = resultType.GetProperty("success");
                bool isSuccess = successProperty != null && (bool)successProperty.GetValue(result);

                if (isSuccess && action != "export")
                {
                    await _context.SaveChangesAsync();
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Create metodu - geliştirilmiş validasyon ve AJAX desteği
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Computer computerModel, bool isAjax = false)
        {
            // Özel validation'lar
            await ValidateComputerAsync(computerModel);

            try
            {
                if (ModelState.IsValid)
                {
                    computerModel.CreatedDate = DateTime.Now;
                    computerModel.LastUpdatedDate = DateTime.Now;
                    computerModel.LastUpdatedBy = User.Identity?.Name ?? "System";

                    SetStatusDisplayProperties(computerModel);

                    _context.Computers.Add(computerModel);
                    await _context.SaveChangesAsync();

                    var message = "Bilgisayar başarıyla eklendi.";

                    if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = message,
                            computer = new
                            {
                                id = computerModel.Id,
                                name = computerModel.Name ?? "",
                                assetTag = computerModel.AssetTag ?? ""
                            }
                        });
                    }

                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Bir hata oluştu: {ex.Message}";

                if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }

                TempData["ErrorMessage"] = errorMessage;
            }

            // AJAX isteğinde model hataları döndür
            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new { success = false, errors = errors });
            }

            await LoadViewBagData();
            return View(computerModel);
        }

        // Geliştirilmiş status update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ComputerStatus status)
        {
            try
            {
                var computer = await _context.Computers.FindAsync(id);
                if (computer == null)
                    return Json(new { success = false, message = "Bilgisayar bulunamadı." });

                var oldStatus = computer.Status;
                computer.Status = status;
                computer.LastUpdatedDate = DateTime.Now;
                computer.LastUpdatedBy = User.Identity?.Name ?? "System";

                SetStatusDisplayProperties(computer);

                await _context.SaveChangesAsync();

                // Audit log eklenebilir
                LogStatusChange(computer.Id, oldStatus, status, User.Identity?.Name ?? "System");

                return Json(new
                {
                    success = true,
                    message = "Durum başarıyla güncellendi.",
                    computer = new
                    {
                        id = computer.Id,
                        status = new
                        {
                            value = (int)computer.Status,
                            name = computer.Status.ToString(),
                            displayName = computer.StatusDisplayName ?? "",
                            badgeClass = computer.StatusBadgeClass ?? ""
                        },
                        lastUpdated = computer.LastUpdatedDate?.ToString("dd.MM.yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Geliştirilmiş CSV export
        public async Task<IActionResult> ExportToCsv(string? searchString = null, string? statusFilter = null,
            string? companyFilter = null, string format = "csv")
        {
            try
            {
                var query = _context.Computers
                    .Include(c => c.Company)
                    .Include(c => c.AssignedEmployee)
                    .AsQueryable();

                // Aynı filtreleri uygula
                query = ApplyFilters(query, searchString, statusFilter, companyFilter);

                var computers = await query.OrderBy(c => c.Name).ToListAsync();

                if (format.ToLower() == "json")
                {
                    return Json(new
                    {
                        success = true,
                        data = computers.Select(c => new
                        {
                            name = c.Name ?? "",
                            assetTag = c.AssetTag ?? "",
                            company = c.Company?.Name ?? "",
                            status = GetStatusDisplayName(c.Status)
                        }),
                        exportDate = DateTime.Now,
                        totalRecords = computers.Count
                    });
                }

                // CSV export
                var csv = GenerateCsv(computers);
                var fileName = $"bilgisayarlar_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

                return File(bytes, "text/csv; charset=utf-8", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export işlemi sırasında hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // API endpoint - filtreleme verileri için
        [HttpGet]
        public async Task<JsonResult> GetFilterData()
        {
            try
            {
                var statusData = Enum.GetValues(typeof(ComputerStatus))
                    .Cast<ComputerStatus>()
                    .Select(status => new
                    {
                        value = (int)status,
                        text = GetStatusDisplayName(status),
                        count = _context.Computers.Count(c => c.Status == status)
                    }).ToList();

                var companiesData = await _context.Companies
                    .Select(c => new
                    {
                        value = c.Id,
                        text = c.Name ?? "",
                        count = _context.Computers.Count(comp => comp.CompanyId == c.Id)
                    })
                    .Where(c => c.count > 0)
                    .OrderBy(c => c.text)
                    .ToListAsync();

                var brandsData = await _context.Computers
                    .Where(c => !string.IsNullOrEmpty(c.Brand))
                    .GroupBy(c => c.Brand)
                    .Select(g => new
                    {
                        value = g.Key ?? "",
                        text = g.Key ?? "",
                        count = g.Count()
                    })
                    .OrderByDescending(b => b.count)
                    .Take(20)
                    .ToListAsync();

                var filterData = new
                {
                    statuses = statusData,
                    companies = companiesData,
                    brands = brandsData
                };

                return Json(new { success = true, data = filterData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Private helper methods - GetFilteredComputersAsync ve diğer eksik metodlar
        private async Task<(List<Computer> Computers, int TotalPages, int TotalCount, object Stats)> GetFilteredComputersAsync(
            string? searchString, string? statusFilter, string? companyFilter, string? sortOrder, int page)
        {
            var computers = _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .AsQueryable();

            // Filtreleri uygula
            computers = ApplyFilters(computers, searchString, statusFilter, companyFilter);

            // Sıralama
            computers = ApplySorting(computers, sortOrder);

            // Sayfalama
            var totalCount = await computers.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)_pageSize);

            var computersForPage = await computers
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Status properties'i set et
            foreach (var computer in computersForPage)
            {
                SetStatusDisplayProperties(computer);
            }

            // İstatistikleri hesapla
            var stats = await CalculateStatsAsync();

            return (computersForPage, totalPages, totalCount, stats);
        }

        private IQueryable<Computer> ApplyFilters(IQueryable<Computer> query, string? searchString,
            string? statusFilter, string? companyFilter)
        {
            // Arama
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c =>
                    (c.Name != null && c.Name.Contains(searchString)) ||
                    (c.AssetTag != null && c.AssetTag.Contains(searchString)) ||
                    (c.SerialNumber != null && c.SerialNumber.Contains(searchString)) ||
                    (c.Brand != null && c.Brand.Contains(searchString)) ||
                    (c.Model != null && c.Model.Contains(searchString)) ||
                    (c.Company != null && c.Company.Name.Contains(searchString)) ||
                    (c.AssignedEmployee != null &&
                     (c.AssignedEmployee.FirstName + " " + c.AssignedEmployee.LastName).Contains(searchString)));
            }

            // Durum filtreleme
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<ComputerStatus>(statusFilter, true, out var status))
                {
                    query = query.Where(c => c.Status == status);
                }
            }

            // Firma filtreleme
            if (!string.IsNullOrEmpty(companyFilter) && int.TryParse(companyFilter, out var companyId))
            {
                query = query.Where(c => c.CompanyId == companyId);
            }

            return query;
        }

        private IQueryable<Computer> ApplySorting(IQueryable<Computer> query, string? sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => query.OrderByDescending(c => c.Name),
                "name_asc" => query.OrderBy(c => c.Name),
                "date_desc" => query.OrderByDescending(c => c.CreatedDate),
                "date_asc" => query.OrderBy(c => c.CreatedDate),
                "status_desc" => query.OrderByDescending(c => c.Status),
                "status_asc" => query.OrderBy(c => c.Status),
                "brand_desc" => query.OrderByDescending(c => c.Brand),
                "brand_asc" => query.OrderBy(c => c.Brand),
                "company_desc" => query.OrderByDescending(c => c.Company!.Name),
                "company_asc" => query.OrderBy(c => c.Company!.Name),
                "warranty_desc" => query.OrderByDescending(c => c.WarrantyEndDate),
                "warranty_asc" => query.OrderBy(c => c.WarrantyEndDate),
                _ => query.OrderByDescending(c => c.CreatedDate)
            };
        }

        private async Task<object> CalculateStatsAsync()
        {
            return new
            {
                Total = await _context.Computers.CountAsync(),
                Active = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.Aktif),
                InUse = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.Kullanımda),
                InPool = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.Havuzda),
                Assigned = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.Zimmetli),
                InMaintenance = await _context.Computers.CountAsync(c => c.Status == ComputerStatus.Bakim),
                Broken = await _context.Computers.CountAsync(c =>
                    c.Status == ComputerStatus.Bozuk || c.Status == ComputerStatus.Arızalı),
                UnderWarranty = await _context.Computers.CountAsync(c =>
                    c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value > DateTime.Now),
                ExpiredWarranty = await _context.Computers.CountAsync(c =>
                    c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value <= DateTime.Now),
                WarrantyExpiringSoon = await _context.Computers.CountAsync(c =>
                    c.WarrantyEndDate.HasValue &&
                    c.WarrantyEndDate.Value > DateTime.Now &&
                    c.WarrantyEndDate.Value <= DateTime.Now.AddDays(90))
            };
        }

        private async Task ValidateComputerAsync(Computer computerModel, int? excludeId = null)
        {
            if (!string.IsNullOrEmpty(computerModel.AssetTag) &&
                await _context.Computers.AnyAsync(c => c.AssetTag == computerModel.AssetTag &&
                (excludeId == null || c.Id != excludeId)))
            {
                ModelState.AddModelError("AssetTag", "Bu varlık etiketi zaten kullanılıyor.");
            }

            if (!string.IsNullOrEmpty(computerModel.SerialNumber) &&
                await _context.Computers.AnyAsync(c => c.SerialNumber == computerModel.SerialNumber &&
                (excludeId == null || c.Id != excludeId)))
            {
                ModelState.AddModelError("SerialNumber", "Bu seri numarası zaten kayıtlı.");
            }

            if (!await _context.Companies.AnyAsync(c => c.Id == computerModel.CompanyId))
            {
                ModelState.AddModelError("CompanyId", "Geçersiz firma seçimi.");
            }

            if (computerModel.AssignedEmployeeId.HasValue &&
                !await _context.Employees.AnyAsync(e => e.Id == computerModel.AssignedEmployeeId.Value))
            {
                ModelState.AddModelError("AssignedEmployeeId", "Geçersiz çalışan seçimi.");
            }
        }

        private StringBuilder GenerateCsv(List<Computer> computers)
        {
            var csv = new StringBuilder();

            // Header
            csv.AppendLine("Ad,Varlık Etiketi,Seri Numarası,Firma,Atanan Çalışan,Durum,Marka,Model," +
                          "İşlemci,RAM (GB),Depolama Tipi,Depolama (GB),İşletim Sistemi,Satın Alma Tarihi," +
                          "Garanti Bitiş Tarihi,Oluşturma Tarihi,Son Güncelleme");

            foreach (var computer in computers)
            {
                var assignedEmployee = computer.AssignedEmployee != null
                    ? $"{computer.AssignedEmployee.FirstName} {computer.AssignedEmployee.LastName}"
                    : "";

                var purchaseDate = computer.PurchaseDate?.ToString("dd.MM.yyyy") ?? "";
                var warrantyDate = computer.WarrantyEndDate?.ToString("dd.MM.yyyy") ?? "";
                var createdDate = computer.CreatedDate.ToString("dd.MM.yyyy HH:mm");
                var updatedDate = computer.LastUpdatedDate?.ToString("dd.MM.yyyy HH:mm") ?? "";

                csv.AppendLine($"\"{computer.Name ?? ""}\",\"{computer.AssetTag ?? ""}\",\"{computer.SerialNumber ?? ""}\"," +
                              $"\"{computer.Company?.Name ?? ""}\",\"{assignedEmployee}\",\"{GetStatusDisplayName(computer.Status)}\"," +
                              $"\"{computer.Brand ?? ""}\",\"{computer.Model ?? ""}\",\"{computer.ProcessorName ?? ""}\"," +
                              $"\"{computer.RamAmount}\",\"{computer.StorageType?.ToString() ?? ""}\",\"{computer.StorageSize}\"," +
                              $"\"{computer.OperatingSystem ?? ""}\",\"{purchaseDate}\",\"{warrantyDate}\"," +
                              $"\"{createdDate}\",\"{updatedDate}\"");
            }

            return csv;
        }

        private void LogStatusChange(int computerId, ComputerStatus oldStatus, ComputerStatus newStatus, string updatedBy)
        {
            // Audit log için - isterseniz ayrı bir AuditLog tablosu oluşturabilirsiniz
            System.Diagnostics.Debug.WriteLine($"Computer {computerId} status changed from {oldStatus} to {newStatus} by {updatedBy}");
        }

        // Bulk işlem helper metodları
        private Task<object> BulkUpdateStatusInternal(List<Computer> computers, object? data)
        {
            if (data == null || !Enum.TryParse(data.ToString(), out ComputerStatus status))
            {
                return Task.FromResult<object>(new { success = false, message = "Geçersiz durum." });
            }

            foreach (var computer in computers)
            {
                computer.Status = status;
                computer.LastUpdatedDate = DateTime.Now;
                computer.LastUpdatedBy = User.Identity?.Name ?? "System";
                SetStatusDisplayProperties(computer);
            }

            return Task.FromResult<object>(new { success = true, message = $"{computers.Count} bilgisayarın durumu güncellendi." });
        }

        private async Task<object> BulkAssignEmployeeInternal(List<Computer> computers, object? data)
        {
            var employeeId = data != null ? (int?)Convert.ToInt32(data) : null;

            if (employeeId.HasValue && !await _context.Employees.AnyAsync(e => e.Id == employeeId.Value))
            {
                return new { success = false, message = "Geçersiz çalışan seçimi." };
            }

            foreach (var computer in computers)
            {
                computer.AssignedEmployeeId = employeeId;
                computer.LastUpdatedDate = DateTime.Now;
                computer.LastUpdatedBy = User.Identity?.Name ?? "System";

                // Çalışan atandığında durumu güncelle
                if (employeeId.HasValue && computer.Status == ComputerStatus.Havuzda)
                {
                    computer.Status = ComputerStatus.Zimmetli;
                }
                else if (!employeeId.HasValue && computer.Status == ComputerStatus.Zimmetli)
                {
                    computer.Status = ComputerStatus.Havuzda;
                }

                SetStatusDisplayProperties(computer);
            }

            var message = employeeId.HasValue
                ? $"{computers.Count} bilgisayar çalışana atandı."
                : $"{computers.Count} bilgisayarın çalışan ataması kaldırıldı.";

            return new { success = true, message = message };
        }

        private async Task<object> BulkUpdateCompanyInternal(List<Computer> computers, object? data)
        {
            if (data == null || !int.TryParse(data.ToString(), out var companyId))
            {
                return new { success = false, message = "Geçersiz firma seçimi." };
            }

            if (!await _context.Companies.AnyAsync(c => c.Id == companyId))
            {
                return new { success = false, message = "Firma bulunamadı." };
            }

            foreach (var computer in computers)
            {
                computer.CompanyId = companyId;
                computer.LastUpdatedDate = DateTime.Now;
                computer.LastUpdatedBy = User.Identity?.Name ?? "System";
            }

            return new { success = true, message = $"{computers.Count} bilgisayarın firmasi güncellendi." };
        }

        private Task<object> BulkDeleteInternal(List<Computer> computers)
        {
            try
            {
                _context.Computers.RemoveRange(computers);
                return Task.FromResult<object>(new { success = true, message = $"{computers.Count} bilgisayar silindi." });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, message = $"Silme hatası: {ex.Message}" });
            }
        }

        private Task<object> BulkExportInternal(List<Computer> computers)
        {
            try
            {
                var csv = GenerateCsv(computers);
                var fileName = $"secili_bilgisayarlar_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

                return Task.FromResult<object>(new
                {
                    success = true,
                    message = "Export başarılı",
                    downloadUrl = $"data:text/csv;charset=utf-8;base64,{Convert.ToBase64String(bytes)}",
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new { success = false, message = $"Export hatası: {ex.Message}" });
            }
        }

        // Mevcut metodlar - Enhanced versiyonlar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateStatus(List<int> computerIds, ComputerStatus status)
        {
            return await BulkAction("updatestatus", computerIds, (int)status);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssignEmployee(List<int> computerIds, int? employeeId)
        {
            return await BulkAction("assignemployee", computerIds, employeeId);
        }

        public async Task<IActionResult> Create()
        {
            await LoadViewBagData();
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
                return NotFound();

            SetStatusDisplayProperties(computer);
            await LoadViewBagData();
            return View(computer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Computer computerModel, bool isAjax = false)
        {
            if (id != computerModel.Id)
                return NotFound();

            // Özel validation'lar (kendi kaydı hariç)
            await ValidateComputerAsync(computerModel, id);

            try
            {
                if (ModelState.IsValid)
                {
                    // Mevcut kaydın CreatedDate'ini koru
                    var existingComputer = await _context.Computers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existingComputer != null)
                    {
                        computerModel.CreatedDate = existingComputer.CreatedDate;
                    }

                    computerModel.LastUpdatedDate = DateTime.Now;
                    computerModel.LastUpdatedBy = User.Identity?.Name ?? "System";

                    SetStatusDisplayProperties(computerModel);

                    _context.Update(computerModel);
                    await _context.SaveChangesAsync();

                    var message = "Bilgisayar başarıyla güncellendi.";

                    if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = true,
                            message = message,
                            computer = new
                            {
                                id = computerModel.Id,
                                name = computerModel.Name ?? "",
                                assetTag = computerModel.AssetTag ?? "",
                                lastUpdated = computerModel.LastUpdatedDate?.ToString("dd.MM.yyyy HH:mm")
                            }
                        });
                    }

                    TempData["SuccessMessage"] = message;
                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ComputerExists(computerModel.Id))
                {
                    var notFoundMessage = "Bilgisayar bulunamadı.";
                    if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = notFoundMessage });
                    }
                    return NotFound();
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Bir hata oluştu: {ex.Message}";

                if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }

                TempData["ErrorMessage"] = errorMessage;
            }

            // AJAX isteğinde model hataları döndür
            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return Json(new { success = false, errors = errors });
            }

            await LoadViewBagData();
            return View(computerModel);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
                return NotFound();

            SetStatusDisplayProperties(computer);
            return View(computer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool isAjax = false)
        {
            try
            {
                var computer = await _context.Computers.FindAsync(id);
                if (computer != null)
                {
                    _context.Computers.Remove(computer);
                    await _context.SaveChangesAsync();

                    var message = "Bilgisayar başarıyla silindi.";

                    if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = message });
                    }

                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    var errorMessage = "Silinecek bilgisayar bulunamadı.";

                    if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }

                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Silme işlemi sırasında hata oluştu: {ex.Message}";

                if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMessage });
                }

                TempData["ErrorMessage"] = errorMessage;
            }

            if (isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
                return NotFound();

            SetStatusDisplayProperties(computer);
            return View(computer);
        }

        // Dashboard ve Raporlama
        public async Task<IActionResult> Dashboard()
        {
            var stats = await CalculateStatsAsync();

            // Firma bazında dağılım
            var companyStats = await _context.Computers
                .Include(c => c.Company)
                .Where(c => c.Company != null)
                .GroupBy(c => c.Company!.Name)
                .Select(g => new { CompanyName = g.Key ?? "", Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Durum bazında dağılım
            var statusStats = await _context.Computers
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Son eklenen bilgisayarlar
            var recentComputers = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .OrderByDescending(c => c.CreatedDate)
                .Take(10)
                .ToListAsync();

            foreach (var computer in recentComputers)
            {
                SetStatusDisplayProperties(computer);
            }

            // Garanti süresi yaklaşan bilgisayarlar
            var expiringSoon = await _context.Computers
                .Include(c => c.Company)
                .Where(c => c.WarrantyEndDate.HasValue &&
                           c.WarrantyEndDate.Value > DateTime.Now &&
                           c.WarrantyEndDate.Value <= DateTime.Now.AddDays(90))
                .OrderBy(c => c.WarrantyEndDate)
                .Take(10)
                .ToListAsync();

            // Marka bazında dağılım
            var brandStats = await _context.Computers
                .Where(c => !string.IsNullOrEmpty(c.Brand))
                .GroupBy(c => c.Brand)
                .Select(g => new { Brand = g.Key ?? "", Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            ViewBag.Stats = stats;
            ViewBag.CompanyStats = companyStats;
            ViewBag.StatusStats = statusStats;
            ViewBag.RecentComputers = recentComputers;
            ViewBag.ExpiringSoon = expiringSoon;
            ViewBag.BrandStats = brandStats;

            return View();
        }

        // Warranty raporları için yeni action'lar
        public async Task<IActionResult> WarrantyReport()
        {
            var computers = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.AssignedEmployee)
                .Where(c => c.WarrantyEndDate.HasValue)
                .OrderBy(c => c.WarrantyEndDate)
                .ToListAsync();

            foreach (var computer in computers)
            {
                SetStatusDisplayProperties(computer);
            }

            return View(computers);
        }

        // Gelişmiş API endpoint'leri
        [HttpGet]
        public async Task<JsonResult> GetComputersByStatus(ComputerStatus status)
        {
            try
            {
                var computers = await _context.Computers
                    .Include(c => c.Company)
                    .Include(c => c.AssignedEmployee)
                    .Where(c => c.Status == status)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name ?? "",
                        assetTag = c.AssetTag ?? "",
                        company = c.Company != null ? c.Company.Name : "",
                        assignedEmployee = c.AssignedEmployee != null
                            ? $"{c.AssignedEmployee.FirstName} {c.AssignedEmployee.LastName}"
                            : null
                    })
                    .ToListAsync();

                return Json(new { success = true, data = computers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetEmployeesByCompany(int companyId)
        {
            try
            {
                var employees = await _context.Employees
                    .Where(e => e.CompanyId == companyId && e.IsActive)
                    .Select(e => new
                    {
                        Id = e.Id,
                        Name = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                        Department = e.Department ?? ""
                    })
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetComputerDetails(int id)
        {
            try
            {
                var computer = await _context.Computers
                    .Include(c => c.Company)
                    .Include(c => c.AssignedEmployee)
                    .Where(c => c.Id == id)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name ?? "",
                        assetTag = c.AssetTag ?? "",
                        serialNumber = c.SerialNumber ?? "",
                        brand = c.Brand ?? "",
                        model = c.Model ?? "",
                        company = c.Company != null ? new
                        {
                            id = c.Company.Id,
                            name = c.Company.Name ?? ""
                        } : null,
                        assignedEmployee = c.AssignedEmployee != null ? new
                        {
                            id = c.AssignedEmployee.Id,
                            name = $"{c.AssignedEmployee.FirstName} {c.AssignedEmployee.LastName}",
                            department = c.AssignedEmployee.Department ?? ""
                        } : null,
                        status = new
                        {
                            value = (int)c.Status,
                            name = c.Status.ToString(),
                            displayName = GetStatusDisplayName(c.Status)
                        },
                        hardware = new
                        {
                            processor = c.ProcessorName ?? "",
                            ram = new
                            {
                                amount = c.RamAmount,
                                type = c.RamType ?? ""
                            },
                            storage = new
                            {
                                size = c.StorageSize,
                                type = c.StorageType != null ? c.StorageType.ToString() : ""
                            },
                            operatingSystem = c.OperatingSystem ?? ""
                        },

                        warranty = new
                        {
                            endDate = c.WarrantyEndDate,
                            daysRemaining = c.WarrantyDaysRemaining,
                            isExpired = c.WarrantyEndDate.HasValue && c.WarrantyEndDate.Value <= DateTime.Now
                        },
                        dates = new
                        {
                            purchase = c.PurchaseDate,
                            created = c.CreatedDate,
                            lastUpdated = c.LastUpdatedDate,
                            lastUpdatedBy = c.LastUpdatedBy ?? ""
                        }
                    })
                    .FirstOrDefaultAsync();

                if (computer == null)
                {
                    return Json(new { success = false, message = "Bilgisayar bulunamadı." });
                }

                return Json(new { success = true, data = computer });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Performance ve caching için
        [HttpGet]
        public async Task<JsonResult> GetCachedStats()
        {
            var stats = await CalculateStatsAsync();
            return Json(new { success = true, data = stats, cachedAt = DateTime.Now });
        }

        // Private helper methods - ViewBag ve diğer utility metodları
        private async Task<bool> ComputerExists(int id)
        {
            return await _context.Computers.AnyAsync(e => e.Id == id);
        }

        private async Task LoadViewBagData()
        {
            ViewBag.StatusList = GetStatusSelectList();
            ViewBag.StorageTypeList = GetStorageTypeSelectList();

            ViewBag.Companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name ?? ""
                })
                .ToListAsync();

            ViewBag.Employees = await _context.Employees
                .Include(e => e.Company)
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.FirstName ?? ""} {e.LastName ?? ""}",
                    Group = new SelectListGroup
                    {
                        Name = e.Company != null ? e.Company.Name : "Diğer"
                    }
                })
                .ToListAsync();
        }

        private async Task LoadFilterData()
        {
            ViewBag.StatusFilterList = new SelectList(
                Enum.GetValues(typeof(ComputerStatus))
                    .Cast<ComputerStatus>()
                    .Select(status => new
                    {
                        Value = ((int)status).ToString(),
                        Text = GetStatusDisplayName(status)
                    }),
                "Value", "Text");

            ViewBag.CompanyFilterList = new SelectList(
                await _context.Companies
                    .Where(c => c.IsActive && _context.Computers.Any(comp => comp.CompanyId == c.Id))
                    .OrderBy(c => c.Name)
                    .Select(c => new { Value = c.Id, Text = c.Name ?? "" })
                    .ToListAsync(),
                "Value", "Text");
        }

        private List<SelectListItem> GetStatusSelectList()
        {
            return Enum.GetValues(typeof(ComputerStatus))
                .Cast<ComputerStatus>()
                .Select(status => new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = GetStatusDisplayName(status)
                })
                .ToList();
        }

        private List<SelectListItem> GetStorageTypeSelectList()
        {
            return Enum.GetValues(typeof(StorageType))
                .Cast<StorageType>()
                .Select(type => new SelectListItem
                {
                    Value = ((int)type).ToString(),
                    Text = type.ToString()
                })
                .ToList();
        }

        private void SetStatusDisplayProperties(Computer computer)
        {
            computer.StatusDisplayName = GetStatusDisplayName(computer.Status);
            computer.StatusBadgeClass = GetStatusBadgeClass(computer.Status);
        }

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

        private string GetStatusBadgeClass(ComputerStatus status)
        {
            return status switch
            {
                ComputerStatus.Aktif => "badge-success",
                ComputerStatus.Kullanımda => "badge-primary",
                ComputerStatus.Zimmetli => "badge-info",
                ComputerStatus.Havuzda => "badge-secondary",
                ComputerStatus.Bakim => "badge-warning",
                ComputerStatus.Arızalı or ComputerStatus.Bozuk => "badge-danger",
                _ => "badge-light"
            };
        }
    }

    // Batch update için DTO class
    public class ComputerBatchUpdate
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ComputerStatus? Status { get; set; }
        public int? CompanyId { get; set; }
        public int? AssignedEmployeeId { get; set; }
        public string? Notes { get; set; }
    }
}