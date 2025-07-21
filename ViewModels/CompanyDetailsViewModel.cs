using System.Collections.Generic;
using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.ViewModels
{
    public class CompanyDetailsViewModel
    {
        public Company Company { get; set; }
        public List<Employee> Employees { get; set; }
        public List<Computer> Computers { get; set; }
        public List<Software> Software { get; set; }
        public List<Supply> Supplies { get; set; }
    }
}
