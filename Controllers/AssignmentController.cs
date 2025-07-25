using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.IO;

namespace WebApplication1.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly AppDbContext _context;

        public AssignmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Assignment/Index - Ana zimmetleme sayfası
        public async Task<IActionResult> Index(string searchString, string status, string personnel, string productType)
        {
            ViewBag.ZimmetDurumlari = new[] { "Tümü", "Zimmet Dışı", "Zimmetli", "Bakımda", "Kayıp" };

            // Firma listesi
            var companies = await _context.Companies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Companies = companies;

            // Ürün türleri
            ViewBag.ProductTypes = new[] { "Tümü", "Bilgisayar", "Yazılım", "Sarf Malzeme", "Diğer" };

            // Zimmetli personellerin listesi
            ViewBag.Personeller = await _context.Items
                .Where(i => !string.IsNullOrEmpty(i.AssignedPersonnel))
                .Select(i => i.AssignedPersonnel)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            var items = from i in _context.Items
                        where i.IsActive == true
                        select i;

            // Arama filtresi
            if (!string.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Name.Contains(searchString) ||
                                        s.SystemBarcode.Contains(searchString) ||
                                        (s.AssignedPersonnel != null && s.AssignedPersonnel.Contains(searchString)));
                ViewBag.SearchString = searchString;
            }

            // Durum filtresi
            if (!string.IsNullOrEmpty(status) && status != "Tümü")
            {
                items = items.Where(x => x.AssignmentStatus == status);
                ViewBag.SelectedStatus = status;
            }

            // Personel filtresi
            if (!string.IsNullOrEmpty(personnel))
            {
                items = items.Where(x => x.AssignedPersonnel == personnel);
                ViewBag.SelectedPersonnel = personnel;
            }

            // Ürün türü filtresi
            if (!string.IsNullOrEmpty(productType) && productType != "Tümü")
            {
                items = items.Where(x => x.Category == productType);
                ViewBag.SelectedProductType = productType;
            }

            var itemList = await items.OrderBy(i => i.AssignmentStatus)
                                     .ThenBy(i => i.Name)
                                     .ToListAsync();

            // ViewModel'e dönüştür (Company bilgisi ile)
            var itemViewModels = itemList.Select(item => new ItemDisplayViewModel
            {
                Id = item.Id,
                Name = item.Name,
                SystemBarcode = item.SystemBarcode,
                Category = item.Category,
                AssignmentStatus = item.AssignmentStatus,
                AssignedPersonnel = item.AssignedPersonnel,
                AssignmentDate = item.AssignmentDate,
                UnitPrice = item.UnitPrice,
                IsCriticalLevel = item.IsCriticalLevel,
                Description = item.Description,
                IsActive = item.IsActive,
                // Company bilgisini ViewBag'den al (basit çözüm - şimdilik boş)
                CompanyName = "N/A" // ItemModel'de CompanyId olmadığı için şimdilik sabit
            }).ToList();

            // İstatistikler için ViewBag
            ViewBag.TotalItems = itemList.Count;
            ViewBag.AvailableItems = itemList.Count(x => x.AssignmentStatus == "Zimmet Dışı");
            ViewBag.AssignedItems = itemList.Count(x => x.AssignmentStatus == "Zimmetli");
            ViewBag.MaintenanceItems = itemList.Count(x => x.AssignmentStatus == "Bakımda");
            ViewBag.LostItems = itemList.Count(x => x.AssignmentStatus == "Kayıp");

            return View(itemViewModels);
        }

        // GET: Assignment/AssignmentPage - Zimmet verme sayfası
        public async Task<IActionResult> AssignmentPage()
        {
            ViewBag.Companies = await _context.Companies
                .Where(c => c.IsActive)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.ProductTypes = new[] { "Tümü", "Bilgisayar", "Yazılım", "Sarf Malzeme", "Diğer" };

            return View();
        }

        // GET: Assignment/ReturnPage - İade alma sayfası
        public async Task<IActionResult> ReturnPage()
        {
            ViewBag.Companies = await _context.Companies
                .Where(c => c.IsActive)
                .Select(c => new { Id = c.Id, Name = c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View();
        }

        // Firma listesi API
        [HttpGet]
        public async Task<IActionResult> GetCompaniesList()
        {
            try
            {
                var companies = await _context.Companies
                    .Where(c => c.IsActive)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        employeeCount = _context.Employees.Count(e => e.CompanyId == c.Id && e.IsActive)
                    })
                    .OrderBy(c => c.name)
                    .ToListAsync();

                return Json(new { success = true, data = companies });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Firma listesi alınırken hata: {ex.Message}" });
            }
        }

        // Firmaya göre çalışan listesi
        [HttpGet]
        public async Task<IActionResult> GetEmployeesByCompany(int companyId)
        {
            try
            {
                if (companyId <= 0)
                {
                    return Json(new { success = false, message = "Geçersiz firma ID" });
                }

                var employees = await _context.Employees
                    .Where(e => e.CompanyId == companyId && e.IsActive)
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.FirstName + " " + e.LastName,
                        department = e.Department,
                        position = e.Position,
                        email = e.Email
                    })
                    .OrderBy(e => e.name)
                    .ToListAsync();

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Çalışan listesi alınırken hata: {ex.Message}" });
            }
        }

        // Havuzdaki ürünler
        [HttpGet]
        public async Task<IActionResult> GetAvailableProducts(string productType)
        {
            try
            {
                var query = _context.Items
                    .Where(i => i.IsActive && i.AssignmentStatus == "Zimmet Dışı");

                if (!string.IsNullOrEmpty(productType) && productType != "Tümü")
                {
                    query = query.Where(i => i.Category == productType);
                }

                var products = await query
                    .Select(i => new
                    {
                        id = i.Id,
                        name = i.Name,
                        barcode = i.SystemBarcode,
                        category = i.Category,
                        unitPrice = i.UnitPrice,
                        isCritical = i.IsCriticalLevel,
                        description = i.Description
                    })
                    .OrderBy(i => i.name)
                    .ToListAsync();

                return Json(new { success = true, data = products, count = products.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Ürün listesi alınırken hata: {ex.Message}" });
            }
        }

        // Zimmetli ürünleri getir (iade için)
        [HttpGet]
        public async Task<IActionResult> GetAssignedProducts(string searchTerm)
        {
            try
            {
                var query = _context.Items
                    .Where(i => i.IsActive && i.AssignmentStatus == "Zimmetli");

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(i => i.Name.Contains(searchTerm) ||
                                           i.SystemBarcode.Contains(searchTerm) ||
                                           i.AssignedPersonnel.Contains(searchTerm));
                }

                var products = await query
                    .Select(i => new
                    {
                        id = i.Id,
                        name = i.Name,
                        barcode = i.SystemBarcode,
                        category = i.Category,
                        assignedPersonnel = i.AssignedPersonnel,
                        assignmentDate = i.AssignmentDate,
                        unitPrice = i.UnitPrice
                    })
                    .OrderBy(i => i.assignedPersonnel)
                    .ThenBy(i => i.name)
                    .ToListAsync();

                return Json(new { success = true, data = products, count = products.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Zimmetli ürün listesi alınırken hata: {ex.Message}" });
            }
        }

        // ZIMMET VERME İŞLEMİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProduct(int productId, int employeeId, string notes)
        {
            try
            {
                // Ürün ve çalışan bilgilerini al
                var product = await _context.Items.FirstOrDefaultAsync(i => i.Id == productId);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);

                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı!" });
                }

                if (employee == null)
                {
                    return Json(new { success = false, message = "Çalışan bulunamadı!" });
                }

                // Basit kontroller
                var validationResult = ValidateAssignmentSimple(product, employee);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                // Zimmet işlemi
                var personnelName = $"{employee.FirstName} {employee.LastName}";
                product.AssignToPersonnel(personnelName, User.Identity.Name ?? "System");

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"'{product.Name}' ürünü {personnelName} adlı çalışana başarıyla zimmet verildi!",
                    data = new
                    {
                        productName = product.Name,
                        employeeName = personnelName,
                        assignmentDate = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Zimmet verme işleminde hata: {ex.Message}"
                });
            }
        }

        // ZIMMET İADE İŞLEMİ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnProduct(int productId, string returnReason, string notes)
        {
            try
            {
                var product = await _context.Items.FirstOrDefaultAsync(i => i.Id == productId);

                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı!" });
                }

                // İade validasyonu
                var validationResult = ValidateReturnSimple(product);
                if (!validationResult.IsValid)
                {
                    return Json(new { success = false, message = validationResult.ErrorMessage });
                }

                var previousPersonnel = product.AssignedPersonnel;

                // İade işlemi
                product.ReturnFromAssignment(User.Identity.Name ?? "System");

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"'{product.Name}' ürününün zimmet iadesi başarıyla alındı! (Önceki: {previousPersonnel})",
                    data = new
                    {
                        productName = product.Name,
                        previousEmployee = previousPersonnel,
                        returnDate = DateTime.Now,
                        newStatus = product.AssignmentStatus
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Zimmet iade işleminde hata: {ex.Message}"
                });
            }
        }

        // TOPLU ZIMMET VERME
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssignProducts(string productIds, int employeeId, string notes)
        {
            try
            {
                if (string.IsNullOrEmpty(productIds))
                {
                    return Json(new { success = false, message = "Ürün seçimi yapılmadı!" });
                }

                var productIdList = productIds.Split(',').Select(int.Parse).ToList();
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Çalışan bulunamadı!" });
                }

                var products = await _context.Items
                    .Where(i => productIdList.Contains(i.Id) && i.IsActive)
                    .ToListAsync();

                var personnelName = $"{employee.FirstName} {employee.LastName}";
                int successCount = 0;
                var errors = new List<string>();
                var successProducts = new List<string>();

                foreach (var product in products)
                {
                    var validationResult = ValidateAssignmentSimple(product, employee);
                    if (!validationResult.IsValid)
                    {
                        errors.Add($"{product.Name}: {validationResult.ErrorMessage}");
                        continue;
                    }

                    // Zimmet ver
                    product.AssignToPersonnel(personnelName, User.Identity.Name ?? "System");
                    successProducts.Add(product.Name);
                    successCount++;
                }

                await _context.SaveChangesAsync();

                var message = $"{successCount} adet ürün {personnelName} adlı çalışana toplu zimmet verildi!";
                if (errors.Any())
                {
                    message += $"\n\nUyarılar ({errors.Count} adet):\n{string.Join("\n", errors.Take(5))}";
                    if (errors.Count > 5) message += $"\n... ve {errors.Count - 5} adet daha";
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    data = new
                    {
                        successCount = successCount,
                        errorCount = errors.Count,
                        employeeName = personnelName,
                        successProducts = successProducts
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Toplu zimmet işleminde hata: {ex.Message}" });
            }
        }

        // ZIMMET DURUMU DEĞİŞTİRME  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeAssignmentStatus(int productId, string newStatus, string reason)
        {
            try
            {
                // İngilizce durum mapping'i ekleyin
                var statusMapping = new Dictionary<string, string>
        {
            { "Zimmet Dışı", "Unassigned" },
            { "Zimmetli", "Assigned" },
            { "Bakımda", "Under Maintenance" },
            { "Kayıp", "Lost" }
        };

                // Türkçe gelirse İngilizce'ye çevir
                var actualStatus = statusMapping.ContainsKey(newStatus) ? statusMapping[newStatus] : newStatus;

                var validStatuses = new[] { "Unassigned", "Assigned", "Under Maintenance", "Lost" };
                if (!validStatuses.Contains(actualStatus))
                {
                    return Json(new { success = false, message = "Geçersiz durum!" });
                }

                var product = await _context.Items.FindAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadı!" });
                }

                var oldStatus = product.AssignmentStatus;
                product.AssignmentStatus = actualStatus;  // İngilizce değer kaydet

                // Eğer durumu "Unassigned" yapılıyorsa zimmet bilgilerini temizle
                if (actualStatus == "Unassigned")
                {
                    product.AssignedPersonnel = null;
                    product.AssignmentDate = null;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"'{product.Name}' ürününün durumu '{newStatus}' olarak güncellendi!",
                    data = new
                    {
                        productName = product.Name,
                        oldStatus = oldStatus,
                        newStatus = actualStatus
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Durum değiştirme işleminde hata: {ex.Message}" });
            }
        }
        // Validasyon metodları
        private ValidationResult ValidateReturnSimple(ItemModel product)
        {
            if (product == null)
                return new ValidationResult { IsValid = false, ErrorMessage = "Ürün bulunamadı!" };

            if (!product.IsActive)
                return new ValidationResult { IsValid = false, ErrorMessage = "Ürün aktif değil!" };

            // İngilizce kontrol
            if (product.AssignmentStatus != "Assigned")  // "Zimmetli" yerine "Assigned"
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Ürün '{product.AssignmentStatus}' durumunda! İade alınamaz."
                };
            }

            if (string.IsNullOrEmpty(product.AssignedPersonnel))
                return new ValidationResult { IsValid = false, ErrorMessage = "Ürün kimseye zimmetlenmemiş!" };

            return new ValidationResult { IsValid = true };
        }

        private ValidationResult ValidateAssignmentSimple(ItemModel product, Employee employee)
        {
            if (product == null || employee == null)
                return new ValidationResult { IsValid = false, ErrorMessage = "Ürün veya çalışan bulunamadı!" };

            if (!product.IsActive)
                return new ValidationResult { IsValid = false, ErrorMessage = "Ürün aktif değil!" };

            if (!employee.IsActive)
                return new ValidationResult { IsValid = false, ErrorMessage = "Çalışan aktif değil!" };

            // İngilizce kontrol
            if (product.AssignmentStatus != "Unassigned")  // "Zimmet Dışı" yerine "Unassigned"
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Ürün zaten '{product.AssignmentStatus}' durumunda! Zimmet verilemez."
                };
            }

            return new ValidationResult { IsValid = true };
        }


        // ZIMMET GEÇMİŞİ
        public async Task<IActionResult> AssignmentHistory(int? employeeId, string personnel, int page = 1, int pageSize = 50)
        {
            // Basit zimmet geçmişi - Sadece Items tablosundaki verilerden
            var query = _context.Items.Where(i => i.IsActive);

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                var employee = await _context.Employees.FindAsync(employeeId.Value);
                if (employee != null)
                {
                    var fullName = $"{employee.FirstName} {employee.LastName}";
                    query = query.Where(i => i.AssignedPersonnel == fullName);
                }
            }

            if (!string.IsNullOrEmpty(personnel))
            {
                query = query.Where(i => i.AssignedPersonnel == personnel);
            }

            var items = await query
                .Where(i => i.AssignmentStatus == "Zimmetli" || !string.IsNullOrEmpty(i.AssignedPersonnel))
                .OrderByDescending(i => i.AssignmentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new AssignmentHistoryViewModel
                {
                    Id = i.Id,
                    ProductName = i.Name,
                    SystemBarcode = i.SystemBarcode,
                    AssignedPersonnel = i.AssignedPersonnel,
                    AssignmentDate = i.AssignmentDate,
                    AssignmentStatus = i.AssignmentStatus,
                    Category = i.Category
                })
                .ToListAsync();

            ViewBag.Employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new { Id = e.Id, Name = e.FirstName + " " + e.LastName })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedPersonnel = personnel;

            return View(items);
        }

        // CSV RAPORU
        public async Task<IActionResult> ExportToCsv(string status, string format = "detailed")
        {
            var query = _context.Items.Where(i => i.IsActive);

            if (!string.IsNullOrEmpty(status) && status != "Tümü")
            {
                query = query.Where(i => i.AssignmentStatus == status);
            }

            var items = await query
                .OrderBy(i => i.AssignmentStatus)
                .ThenBy(i => i.Name)
                .ToListAsync();

            var csv = new StringBuilder();

            if (format == "summary")
            {
                // Özet rapor
                csv.AppendLine("ZİMMET ÖZETİ");
                csv.AppendLine();
                csv.AppendLine("Durum,Adet,Toplam Değer");

                var summary = items.GroupBy(i => i.AssignmentStatus)
                                  .Select(g => new { Status = g.Key, Count = g.Count(), Value = g.Sum(i => i.UnitPrice ?? 0) })
                                  .ToList();

                foreach (var item in summary)
                {
                    csv.AppendLine($"{item.Status},{item.Count},{item.Value:C}");
                }
            }
            else
            {
                // Detaylı rapor başlıkları
                csv.AppendLine("Ürün Adı,Sistem Barkodu,Kategori,Durum,Zimmetli Personel,Zimmet Tarihi,Birim Fiyat,Kritik Seviye,Açıklama");

                // Veri satırları
                foreach (var item in items)
                {
                    csv.AppendLine($"{EscapeCsvField(item.Name)}," +
                                  $"{EscapeCsvField(item.SystemBarcode)}," +
                                  $"{EscapeCsvField(item.Category)}," +
                                  $"{EscapeCsvField(item.AssignmentStatus)}," +
                                  $"{EscapeCsvField(item.AssignedPersonnel ?? "-")}," +
                                  $"{item.AssignmentDate?.ToString("dd.MM.yyyy") ?? "-"}," +
                                  $"{item.UnitPrice ?? 0}," +
                                  $"{(item.IsCriticalLevel ? "Evet" : "Hayır")}," +
                                  $"{EscapeCsvField(item.Description ?? "-")}");
                }
            }

            var fileName = $"ZimmetRaporu_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var contentType = "text/csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, contentType, fileName);
        }

        // CSV için özel karakter escape işlemi
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        // PERSONEL BAZLI GÖRÜNÜM
        public async Task<IActionResult> Personnel()
        {
            var assignedItems = await _context.Items
                .Where(i => i.AssignmentStatus == "Zimmetli" && !string.IsNullOrEmpty(i.AssignedPersonnel))
                .GroupBy(i => i.AssignedPersonnel)
                .Select(g => new PersonnelAssignmentViewModel
                {
                    PersonnelName = g.Key,
                    AssignedItems = g.ToList(),
                    ItemCount = g.Count(),
                    TotalValue = g.Sum(i => i.UnitPrice ?? 0),
                    LastAssignmentDate = g.Max(i => i.AssignmentDate)
                })
                .OrderBy(p => p.PersonnelName)
                .ToListAsync();

            return View(assignedItems);
        }

        // RAPORLAR
        public async Task<IActionResult> Reports()
        {
            var model = new AssignmentReportViewModel
            {
                TotalItems = await _context.Items.CountAsync(i => i.IsActive),
                AvailableItems = await _context.Items.CountAsync(i => i.AssignmentStatus == "Zimmet Dışı" && i.IsActive),
                AssignedItems = await _context.Items.CountAsync(i => i.AssignmentStatus == "Zimmetli"),
                MaintenanceItems = await _context.Items.CountAsync(i => i.AssignmentStatus == "Bakımda"),
                LostItems = await _context.Items.CountAsync(i => i.AssignmentStatus == "Kayıp"),

                ThisMonthAssignments = await _context.Items
                    .Where(i => i.AssignmentDate.HasValue &&
                               i.AssignmentDate.Value.Month == DateTime.Now.Month &&
                               i.AssignmentDate.Value.Year == DateTime.Now.Year)
                    .CountAsync(),

                CriticalAssignedItems = await _context.Items
                    .Where(i => i.AssignmentStatus == "Zimmetli" && i.IsCriticalLevel)
                    .CountAsync(),

                TopPersonnel = await _context.Items
                    .Where(i => i.AssignmentStatus == "Zimmetli" && !string.IsNullOrEmpty(i.AssignedPersonnel))
                    .GroupBy(i => i.AssignedPersonnel)
                    .Select(g => new TopPersonnelViewModel
                    {
                        PersonnelName = g.Key,
                        ItemCount = g.Count(),
                        TotalValue = g.Sum(i => i.UnitPrice ?? 0)
                    })
                    .OrderByDescending(p => p.ItemCount)
                    .Take(10)
                    .ToListAsync(),

                CategoryAssignments = await _context.Items
                    .Where(i => i.AssignmentStatus == "Zimmetli")
                    .GroupBy(i => i.Category)
                    .Select(g => new CategoryAssignmentViewModel
                    {
                        Category = g.Key,
                        AssignedCount = g.Count(),
                        TotalCount = _context.Items.Count(i => i.Category == g.Key && i.IsActive)
                    })
                    .OrderByDescending(c => c.AssignedCount)
                    .ToListAsync()
            };

            return View(model);
        }
    }

    // Validasyon sonuç modeli
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    // ViewModel sınıfları
    public class ItemDisplayViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemBarcode { get; set; }
        public string Category { get; set; }
        public string AssignmentStatus { get; set; }
        public string AssignedPersonnel { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public decimal? UnitPrice { get; set; }
        public bool IsCriticalLevel { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string CompanyName { get; set; }

        // Company özelliği için mock obje
        public CompanyViewModel Company => new CompanyViewModel { Name = CompanyName };
    }

    public class CompanyViewModel
    {
        public string Name { get; set; }
    }

    public class AssignmentHistoryViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string SystemBarcode { get; set; }
        public string AssignedPersonnel { get; set; }
        public DateTime? AssignmentDate { get; set; }
        public string AssignmentStatus { get; set; }
        public string Category { get; set; }
    }

    public class PersonnelAssignmentViewModel
    {
        public string PersonnelName { get; set; }
        public List<ItemModel> AssignedItems { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime? LastAssignmentDate { get; set; }
    }

    public class AssignmentReportViewModel
    {
        public int TotalItems { get; set; }
        public int AvailableItems { get; set; }
        public int AssignedItems { get; set; }
        public int MaintenanceItems { get; set; }
        public int LostItems { get; set; }
        public int ThisMonthAssignments { get; set; }
        public int CriticalAssignedItems { get; set; }
        public List<TopPersonnelViewModel> TopPersonnel { get; set; }
        public List<CategoryAssignmentViewModel> CategoryAssignments { get; set; }

        public double AssignmentRate => TotalItems > 0 ? (double)AssignedItems / TotalItems * 100 : 0;
    }

    public class TopPersonnelViewModel
    {
        public string PersonnelName { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class CategoryAssignmentViewModel
    {
        public string Category { get; set; }
        public int AssignedCount { get; set; }
        public int TotalCount { get; set; }
        public double AssignmentRate => TotalCount > 0 ? (double)AssignedCount / TotalCount * 100 : 0;
    }
}

public class AssignmentHistoryViewModel
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public string SystemBarcode { get; set; }
    public string AssignedPersonnel { get; set; }
    public DateTime? AssignmentDate { get; set; }
    public string AssignmentStatus { get; set; }
    public string Category { get; set; }
}

public class PersonnelAssignmentViewModel
{
    public string PersonnelName { get; set; }
    public List<ItemModel> AssignedItems { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? LastAssignmentDate { get; set; }
}

public class AssignmentReportViewModel
{
    public int TotalItems { get; set; }
    public int AvailableItems { get; set; }
    public int AssignedItems { get; set; }
    public int MaintenanceItems { get; set; }
    public int LostItems { get; set; }
    public int ThisMonthAssignments { get; set; }
    public int CriticalAssignedItems { get; set; }
    public List<TopPersonnelViewModel> TopPersonnel { get; set; }
    public List<CategoryAssignmentViewModel> CategoryAssignments { get; set; }

    public double AssignmentRate => TotalItems > 0 ? (double)AssignedItems / TotalItems * 100 : 0;
}

public class TopPersonnelViewModel
{
    public string PersonnelName { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
}

public class CategoryAssignmentViewModel
{
    public string Category { get; set; }
    public int AssignedCount { get; set; }
    public int TotalCount { get; set; }
    public double AssignmentRate => TotalCount > 0 ? (double)AssignedCount / TotalCount * 100 : 0;
}