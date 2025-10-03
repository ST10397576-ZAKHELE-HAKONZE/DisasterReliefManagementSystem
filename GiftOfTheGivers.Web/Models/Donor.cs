using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models
{
    public class Donor
    {
        [Key]
        public int DonorID { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        public required string ContactNumber { get; set; }

        [Required]
        public required string Email { get; set; }

        // Navigation property for donations (Collection)
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}