using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;
using WebApplication1.Services;
using WebApplication1.EnvanterLib;

namespace WebApplication1.Controllers
{
    public class ItemsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IActivityLogger _activityLogger;

        public ItemsController(AppDbContext context, IActivityLogger activityLogger)
        {
            _context = context;
            _activityLogger = activityLogger;

        }
        // GET: Items
       public async Task<IActionResult> Index(string searchString, string category)
{
    ViewBag.Kategoriler = await _context.Items
        .Where(i => i.IsActive) // 👈 Sadece aktif olanlar
        .Select(i => i.Category)
        .Distinct()
        .OrderBy(k => k)
        .ToListAsync();

    var items = _context.Items
        .Where(i => i.IsActive) // 👈 Aktif ürünler filtreleniyor
        .AsQueryable();

    if (!string.IsNullOrEmpty(searchString))
    {
        items = items.Where(s => s.Name!.Contains(searchString) ||
                                s.Description!.Contains(searchString) ||
                                s.SystemBarcode!.Contains(searchString));
        ViewBag.SearchString = searchString;
    }

    if (!string.IsNullOrEmpty(category))
    {
        items = items.Where(x => x.Category == category);
        ViewBag.SelectedCategory = category;
    }

    return View(await items.OrderByDescending(i => i.CreatedDate).ToListAsync());
}


        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create()
        {
            ViewBag.Kategoriler = GetCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemModel item)
        {
            // Debug: ModelState kontrolü
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Barkod kontrolü
                    if (await _context.Items.AnyAsync(i => i.SystemBarcode == item.SystemBarcode))
                    {
                        ModelState.AddModelError("SystemBarcode", "Bu barkod zaten kullanılmaktadır. Lütfen farklı bir barkod giriniz.");
                        ViewBag.Kategoriler = GetCategories();
                        return View(item);
                    }

                    // Oluşturma bilgileri
                    item.CreatedBy = User.Identity!.Name ?? "System";
                    item.CreatedDate = DateTime.Now;

                    item.CheckCriticalLevel();

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();

                    // LOG EKLENDİ
                    string? userId = this.GetUserFromHttpContext()?.Id.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _activityLogger.LogAsync(userId, "Ürün oluşturuldu", "Item", item.Id);
                    }

                    TempData["Success"] = "Urun basariyla olusturuldu!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Create Error: {ex.Message}");
                    TempData["Error"] = "Ürün kaydedilirken hata oluştu: " + ex.Message;
                }
            }

            ViewBag.Kategoriler = GetCategories();
            return View(item);
        }

        // GET: Items/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            ViewBag.Kategoriler = GetCategories();
            ViewBag.ZimmetDurumlari = new[] { "Zimmet Dışı", "Zimmetli", "Bakımda", "Kayıp" };

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemModel item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            // Debug: ModelState kontrolü
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Aynı barkod başka üründe var mı kontrol et
                    if (await _context.Items.AnyAsync(i => i.SystemBarcode == item.SystemBarcode && i.Id != item.Id))
                    {
                        ModelState.AddModelError("SystemBarcode", "Bu barkod zaten kullanılmaktadır. Lütfen farklı bir barkod giriniz.");
                        ViewBag.Kategoriler = GetCategories();
                        ViewBag.ZimmetDurumlari = new[] { "Zimmet Dışı", "Zimmetli", "Bakımda", "Kayıp" };
                        return View(item);
                    }

                    var originalItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

                    // Orijinal alanları koru
                    item.UpdatedDate = DateTime.Now;
                    item.UpdatedBy = User.Identity!.Name ?? "System";
                    item.CreatedDate = originalItem!.CreatedDate;
                    item.CreatedBy = originalItem.CreatedBy;

                    item.CheckCriticalLevel();

                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    // LOG EKLENDİ
                    string? userId = this.GetUserFromHttpContext()?.Id.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _activityLogger.LogAsync(userId, "Ürün güncellendi", "Item", item.Id);
                    }

                    TempData["Success"] = "Urun basariyla guncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Edit Error: {ex.Message}");
                    TempData["Error"] = "Ürün güncellenirken hata oluştu: " + ex.Message;
                }
            }

            ViewBag.Kategoriler = GetCategories();
            ViewBag.ZimmetDurumlari = new[] { "Zimmet Dışı", "Zimmetli", "Bakımda", "Kayıp" };
            return View(item);
        }

        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // POST: Items/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    try
    {
        var item = await _context.Items.FindAsync(id);

        if (item != null)
        {
            item.IsActive = false; // 👈 Artık fiziksel silme yok
            item.UpdatedDate = DateTime.Now;
            item.UpdatedBy = User.Identity?.Name ?? "System";
            await _context.SaveChangesAsync();

            // LOG EKLENDİ
            string? userId = this.GetUserFromHttpContext()?.Id.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                await _activityLogger.LogAsync(userId, "Ürün pasif hale getirildi (soft delete)", "Item", item.Id);
            }

            TempData["Success"] = "Ürün başarıyla pasif hale getirildi!";
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Delete Error: {ex.Message}");
        TempData["Error"] = "Ürün silinirken hata oluştu: " + ex.Message;
    }

    return RedirectToAction(nameof(Index));
}

        // POST: Items/AssignToPersonnel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToPersonnel(int id, string personnel)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null && !string.IsNullOrEmpty(personnel))
                {
                    item.AssignToPersonnel(personnel, User.Identity!.Name ?? "System");
                    await _context.SaveChangesAsync();

                    // ✅ LOG EKLENDİ
                    string? userId = this.GetUserFromHttpContext()?.Id.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _activityLogger.LogAsync(userId, $"Ürün {personnel} adlı personele zimmetlendi", "Item", item.Id);
                    }

                    TempData["Success"] = $"Ürün {personnel} adlı personele zimmet verildi!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Assign Error: {ex.Message}");
                TempData["Error"] = "Zimmet verme işleminde hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }
        // POST: Items/ReturnFromAssignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnFromAssignment(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null)
                {
                    var previousPersonnel = item.AssignedPersonnel;
                    item.ReturnFromAssignment(User.Identity!.Name ?? "System");
                    await _context.SaveChangesAsync();

                    // ✅ LOG EKLENDİ
                    string? userId = this.GetUserFromHttpContext()?.Id.ToString();

                    if (!string.IsNullOrEmpty(userId))
                    {
                        await _activityLogger.LogAsync(userId, $"Ürün {previousPersonnel} adlı personelden iade alındı", "Item", item.Id);
                    }

                    TempData["Success"] = "Ürün zimmet iadesi yapıldı!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Return Error: {ex.Message}");
                TempData["Error"] = "Zimmet iade işleminde hata oluştu: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }

        private string[] GetCategories()
        {
            return new string[]
            {
                "Bilgisayar Malzemeleri",
                "Kırtasiye Malzemeleri",
                "Temizlik Malzemeleri",
                "Elektrik Malzemeleri",
                "Ofis Mobilyaları",
                "Yazılım Lisansları",
                "Ağ Ekipmanları",
                "Güvenlik Ekipmanları",
                "Mutfak Malzemeleri",
                "Diğer"
            };
        }
    }
}