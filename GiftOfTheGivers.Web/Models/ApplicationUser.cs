using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models 
{
    // Inherit from IdentityUser to get the default Identity fields (Email, PasswordHash, etc.)
    public class ApplicationUser : IdentityUser
    {
        // Add your custom properties here
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [StringLength(13)] // Added a max length appropriate for an ID number
        public string IDNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string UserType { get; set; }
    }

}