using WebApplication1.Models;
using System.Diagnostics;
using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.ViewModels
{
    public class EmployeeDetailViewModel
    {
        public Employee Employee { get; set; } = new Employee();
        public List<Computer> Computers { get; set; } = new List<Computer>();
        public List<Software> Softwares { get; set; } = new List<Software>();
        public List<Supply> Supplies { get; set; } = new List<Supply>();
    }
}