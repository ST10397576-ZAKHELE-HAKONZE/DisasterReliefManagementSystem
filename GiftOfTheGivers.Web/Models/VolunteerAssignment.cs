using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models; // Assuming ApplicationUser and ReliefProject are here

namespace GiftOfTheGivers.Web.Models
{
    public class VolunteerAssignment
    {
        [Key]
        public int AssignmentID { get; set; } // Primary Key

        [Display(Name = "Role/Task")]
        public required VolunteerRole Role { get; set; }

        [Display(Name = "Assigned Date")]
        public System.DateTime AssignedDate { get; set; } = System.DateTime.UtcNow;

        public required AssignmentStatus Status { get; set; }

        // Foreign Keys
        // Note: Using 'required' keyword for non-nullable references is good for C# 8+/EF Core 6+

        [Display(Name = "Volunteer User")]
        public required string UserId { get; set; }

        [Display(Name = "Relief Project")]
        public int ProjectID { get; set; }

        // Navigation properties
        public required virtual ApplicationUser User { get; set; }

        public required virtual ReliefProject Project { get; set; }
    }
}
