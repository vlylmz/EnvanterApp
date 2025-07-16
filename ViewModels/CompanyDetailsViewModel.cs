using WebApplication1.Models;
using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class CompanyDetailsViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public int EmployeeCount { get; set; }
        public int ComputerCount { get; set; }
        public int SoftwareCount { get; set; }
        public int SupplyCount { get; set; }

        public List<Employee> Employees { get; set; } = new();
        public List<Computer> Computers { get; set; } = new();
        public List<Software> Softwares { get; set; } = new();
        public List<Supply> Supplies { get; set; } = new();
    }
}
