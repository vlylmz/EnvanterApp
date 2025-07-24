using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class ItemsController : Controller
    {
        private readonly AppDbContext _context;

        public ItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index(string searchString, string category)
        {
            ViewBag.Kategoriler = await _context.Items
                .Select(i => i.Category)
                .Distinct()
                .OrderBy(k => k)
                .ToListAsync();

            var items = from i in _context.Items
                        select i;

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

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemModel item)
        {
            // Debug: ModelState kontrolü
            if (!ModelState.IsValid)
            {
                // Hataları logla
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Field: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if barcode already exists
                    if (await _context.Items.AnyAsync(i => i.SystemBarcode == item.SystemBarcode))
                    {
                        ModelState.AddModelError("SystemBarcode", "Bu barkod zaten kullanılmaktadır. Lütfen farklı bir barkod giriniz.");
                        ViewBag.Kategoriler = GetCategories();
                        return View(item);
                    }

                    // Set audit fields
                    item.CreatedBy = User.Identity!.Name ?? "System";
                    item.CreatedDate = DateTime.Now;
                    
                    // Check critical level
                    item.CheckCriticalLevel();

                    _context.Items.Add(item);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Urun basariyla olusturuldu!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata logla
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

        // POST: Items/Edit/5
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
                    // Check if barcode already exists (excluding current item)
                    if (await _context.Items.AnyAsync(i => i.SystemBarcode == item.SystemBarcode && i.Id != item.Id))
                    {
                        ModelState.AddModelError("SystemBarcode", "Bu barkod zaten kullanılmaktadır. Lütfen farklı bir barkod giriniz.");
                        ViewBag.Kategoriler = GetCategories();
                        ViewBag.ZimmetDurumlari = new[] { "Zimmet Dışı", "Zimmetli", "Bakımda", "Kayıp" };
                        return View(item);
                    }

                    var originalItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    
                    // Preserve original values
                    item.UpdatedDate = DateTime.Now;
                    item.UpdatedBy = User.Identity!.Name ?? "System";
                    item.CreatedDate = originalItem!.CreatedDate;
                    item.CreatedBy = originalItem.CreatedBy;
                    
                    // Check critical level
                    item.CheckCriticalLevel();

                    _context.Update(item);
                    await _context.SaveChangesAsync();
                    
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
                var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);

                if (item != null)
                {
                    _context.Items.Remove(item);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Urun basariyla silindi!";
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