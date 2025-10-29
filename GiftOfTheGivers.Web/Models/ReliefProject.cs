using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ReliefProject
{
    [Key]
    public int ProjectID { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Location { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public ProjectStatus Status { get; set; }

    // THE CORRECT, UNCOMMENTED DATE FIELDS
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)] // EndDate is nullable (optional)
    public DateTime? EndDate { get; set; }


    [Required]
    [Display(Name = "Coordinator")]
    public required string CoordinatorId { get; set; }

    [ForeignKey("CoordinatorId")]
    public virtual ApplicationUser? Coordinator { get; set; }

    public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    public ICollection<VolunteerAssignment> Assignments { get; set; } = new List<VolunteerAssignment>();
}