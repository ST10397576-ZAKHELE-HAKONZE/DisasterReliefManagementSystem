using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    // Inherit from IdentityUser to get the default Identity fields (Email, PasswordHash, etc.)
    public class ApplicationUser : IdentityUser
    {
        // Add the 'required' keyword to satisfy the compiler and enforce [Required]
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        [StringLength(13)] // Added a max length appropriate for an ID number
        public required string IDNumber { get; set; }

        [Required]
        public required DateTime DateOfBirth { get; set; }

        [Required]
        public required string Gender { get; set; }

        [Required]
        public required string UserType { get; set; }
    }
}