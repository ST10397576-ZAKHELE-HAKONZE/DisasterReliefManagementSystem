using GiftOfTheGivers.Web.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class ReliefProject
{
    [Key]
    public int ProjectID { get; set; } // Primary Key
    public string Name { get; set; }
    public string Location { get; set; }
    public System.DateTime StartDate { get; set; }
    public System.DateTime EndDate { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }

    // Foreign Key (Coordinator)
    public string CoordinatorId { get; set; }
    public virtual ApplicationUser Coordinator { get; set; }

    // Navigation Properties
    public ICollection<Donation> Donations { get; set; }
    public ICollection<VolunteerAssignment> Assignments { get; set; }
}