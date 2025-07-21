using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [MaxLength(100)]
        public override string? Email { get; set; }

        // IdentityUser zaten PasswordHash içeriyor, tekrar tanımlamaya gerek yok
        public override bool TwoFactorEnabled { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public string? UserRole { get; set; } // "Süper Admin", "Admin", "Employee"
    }
}
