using WebApplication1.Models;

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