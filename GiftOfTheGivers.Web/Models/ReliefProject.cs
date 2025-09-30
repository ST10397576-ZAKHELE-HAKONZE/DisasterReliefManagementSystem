using GiftOfTheGivers.Web.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


public class ReliefProject
{
    [Key]
    public int ProjectID { get; set; }

    // Fix the string properties
    public required string Name { get; set; } // CS8618 Warning
    public required string Location { get; set; } // CS8618 Warning
    // ...
    public required string Description { get; set; } // CS8618 Warning
    public required string Status { get; set; } // CS8618 Warning

    // Fix the Coordinator FK/Navigation properties
    public required string CoordinatorId { get; set; } // CS8618 Warning
    public required virtual ApplicationUser Coordinator { get; set; } // CS8618 Warning

    // Fix the ICollection (Navigation properties)
    public ICollection<Donation> Donations { get; set; } = new List<Donation>(); // Initialized, so 'required' is NOT needed
    public ICollection<VolunteerAssignment> Assignments { get; set; } = new List<VolunteerAssignment>(); // Initialized, so 'required' is NOT needed
}