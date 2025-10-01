using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models; // Already present, good!

public class VolunteerAssignment
{
    [Key]
    public int AssignmentID { get; set; } // Primary Key

    // Add 'required' to the non-nullable string properties
    public required string Role { get; set; }

    // DateTime is non-nullable, but it is initialized with a default value, so 'required' is NOT needed
    public System.DateTime AssignedDate { get; set; } = System.DateTime.UtcNow;

    public required string Status { get; set; }

    // Foreign Keys
    public required string UserId { get; set; }

    // Navigation properties must be 'required' if they are non-nullable
    public required virtual ApplicationUser User { get; set; }

    public int ProjectID { get; set; }
    public required virtual ReliefProject Project { get; set; }
}