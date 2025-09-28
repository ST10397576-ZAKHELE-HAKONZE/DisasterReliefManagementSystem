using System.ComponentModel.DataAnnotations;

public class VolunteerAssignment
{
    [Key]
    public int AssignmentID { get; set; } // Primary Key
    public string Role { get; set; }
    public System.DateTime AssignedDate { get; set; } = System.DateTime.UtcNow;
    public string Status { get; set; }

    // Foreign Keys
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }

    public int ProjectID { get; set; }
    public virtual ReliefProject Project { get; set; }
}
