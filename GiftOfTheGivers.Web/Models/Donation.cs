using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiftOfTheGivers.Web.Models
{
    public class Donation
    {
        [Key]
        public int DonationID { get; set; }

        [Required]
        public required string Type { get; set; } // e.g., "Cash", "Goods"

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Ensure precise money format in SQL
        public decimal Amount { get; set; } // Use 0 for goods donations, or a value.

        [Required]
        public required string Description { get; set; } // Detail of goods, e.g., "5 boxes of canned food"

        public DateTime DateReceived { get; set; } = DateTime.UtcNow;

        [Required]
        public required string Status { get; set; } // e.g., "Received", "In Transit", "Allocated"

        // Foreign Key to Donor
        public int DonorID { get; set; }
        public required virtual Donor Donor { get; set; }

        // Foreign Key to ApplicationUser (Staff member who logged the donation)
        [Required]
        public required string RecordedByUserId { get; set; }
        public required virtual ApplicationUser RecordedByUser { get; set; }

        public int? ReliefProjectProjectID { get; set; }

        [ForeignKey("ReliefProjectProjectID")]
        public ReliefProject? ReliefProject { get; set; }
    }
}