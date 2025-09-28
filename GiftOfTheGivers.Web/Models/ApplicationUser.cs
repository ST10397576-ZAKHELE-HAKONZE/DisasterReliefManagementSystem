using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

// Extends IdentityUser to add custom properties
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public string Skills { get; set; }
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

    // Navigation Properties (Relationships)
    public ICollection<Donation> Donations { get; set; }
    public ICollection<VolunteerAssignment> Assignments { get; set; }
    public ICollection<IncidentReport> ReportedIncidents { get; set; }
    public ICollection<ReliefProject> CoordinatedProjects { get; set; }
}