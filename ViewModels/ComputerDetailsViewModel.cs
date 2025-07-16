using System;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class ComputerDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssetTag { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // İlişkili bilgiler
        public string AssignedUserFullName { get; set; }  // Örn: "Ali Yılmaz"
        public string AssignedUserEmail { get; set; }

        public List<Software> InstalledSoftwares { get; set; } = new();
        public List<Supply> Accessories { get; set; } = new();
    }
}