using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

public class SoftwareController : Controller
{
    private static List<Software> softwares = new List<Software>()
    {
        new Software { Id=1, Name="Photoshop", Brand="Adobe", CompanyId=1, CompanyName="Spilaş", AssignedEmployeeId=1, AssignedEmployeeName="Ali Yılmaz", PurchaseDate=DateTime.Today.AddYears(-1), ExpiryDate=DateTime.Today.AddMonths(5), Status="Aktif" },
        new Software { Id=2, Name="Office 365", Brand="Microsoft", CompanyId=2, CompanyName="Manisa Enerji", AssignedEmployeeId=null, AssignedEmployeeName=null, PurchaseDate=DateTime.Today.AddMonths(-8), ExpiryDate=DateTime.Today.AddDays(25), Status="Yaklaşan" }
    };

    private static List<(int Id, string Name)> companies = new()
    {
        (1, "Spilaş"),
        (2, "Manisa Enerji")
    };

    private static List<(int Id, string Name, int CompanyId)> employees = new()
    {
        (1, "Ali Yılmaz", 1),
        (2, "Ayşe Demir", 2)
    };

    public IActionResult Index(string search, int? companyId, int? assignedEmployeeId, string status)
    {
        var list = softwares.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            list = list.Where(s => s.Name.Contains(search) || s.Brand.Contains(search));
        if (companyId.HasValue)
            list = list.Where(s => s.CompanyId == companyId.Value);
        if (assignedEmployeeId.HasValue)
            list = list.Where(s => s.AssignedEmployeeId == assignedEmployeeId.Value);
        if (!string.IsNullOrEmpty(status))
            list = list.Where(s => s.Status == status);

        foreach (var sw in list)
        {
            sw.Status = GetStatus(sw.ExpiryDate);
        }

        ViewBag.Companies = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
        ViewBag.Employees = employees.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name }).ToList();

        return View(list.ToList());
    }

    public IActionResult Create()
    {
        ViewBag.Companies = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
        ViewBag.Employees = employees.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name }).ToList();
        return View();
    }

    [HttpPost]
    public IActionResult Create(Software model)
    {
        if (ModelState.IsValid)
        {
            model.Id = softwares.Count > 0 ? softwares.Max(x => x.Id) + 1 : 1;
            var company = companies.FirstOrDefault(c => c.Id == model.CompanyId);
            model.CompanyName = company.Name;

            if (model.AssignedEmployeeId.HasValue)
            {
                var emp = employees.FirstOrDefault(e => e.Id == model.AssignedEmployeeId.Value);
                model.AssignedEmployeeName = emp.Name;
            }
            else
            {
                model.AssignedEmployeeName = null;
            }
            model.Status = GetStatus(model.ExpiryDate);
            softwares.Add(model);
            return RedirectToAction("Index");
        }
        ViewBag.Companies = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
        ViewBag.Employees = employees.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name }).ToList();
        return View(model);
    }

    public IActionResult Details(int id)
    {
        var sw = softwares.FirstOrDefault(s => s.Id == id);
        if (sw == null) return NotFound();
        return View(sw);
    }

    public IActionResult Delete(int id)
    {
        var sw = softwares.FirstOrDefault(s => s.Id == id);
        if (sw != null) softwares.Remove(sw);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var sw = softwares.FirstOrDefault(s => s.Id == id);
        if (sw == null) return NotFound();
        ViewBag.Companies = companies.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
        ViewBag.Employees = employees.Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Name }).ToList();
        return View(sw);
    }

    [HttpPost]
    public IActionResult Edit(Software model)
    {
        var sw = softwares.FirstOrDefault(s => s.Id == model.Id);
        if (sw == null) return NotFound();

        sw.Name = model.Name;
        sw.Brand = model.Brand;
        sw.CompanyId = model.CompanyId;

        var company = companies.FirstOrDefault(c => c.Id == model.CompanyId);
        sw.CompanyName = company.Name;

        sw.AssignedEmployeeId = model.AssignedEmployeeId;
        if (model.AssignedEmployeeId.HasValue)
        {
            var emp = employees.FirstOrDefault(e => e.Id == model.AssignedEmployeeId.Value);
            sw.AssignedEmployeeName = emp.Name;
        }
        else
        {
            sw.AssignedEmployeeName = null;
        }
        sw.PurchaseDate = model.PurchaseDate;
        sw.ExpiryDate = model.ExpiryDate;
        sw.Status = GetStatus(model.ExpiryDate);

        return RedirectToAction("Index");
    }

    private string GetStatus(DateTime expiryDate)
    {
        var kalanGun = (expiryDate - DateTime.Today).TotalDays;
        if (kalanGun < 0)
            return "Süresi Dolmuş";
        if (kalanGun <= 30)
            return "Yaklaşan";
        return "Aktif";
    }
}
