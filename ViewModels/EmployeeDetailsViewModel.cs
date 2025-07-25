﻿using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class EmployeeDetailViewModel
    {
        public Employee Employee { get; set; }
        public List<Computer> Computers { get; set; }
        public List<Software> Softwares { get; set; }
        public List<Supply> Supplies { get; set; }

        public EmployeeDetailViewModel()
        {
            Computers = new List<Computer>();
            Softwares = new List<Software>();
            Supplies = new List<Supply>();
        }
    }
}
