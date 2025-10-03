using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    /// <summary>
    /// Defines the specific roles or tasks a volunteer may be assigned to.
    /// </summary>
    public enum VolunteerRole
    {
        [Display(Name = "Logistics Coordinator")]
        Logistics = 1,

        [Display(Name = "Field Support")]
        FieldSupport = 2,

        [Display(Name = "Administrative Support")]
        AdminSupport = 3,

        [Display(Name = "Donation Sorter")]
        Sorter = 4,

        [Display(Name = "Public Relations/Outreach")]
        Outreach = 5
    }

    /// <summary>
    /// Defines the current status of a volunteer's assignment.
    /// </summary>
    public enum AssignmentStatus
    {
        [Display(Name = "Pending Confirmation")]
        Pending = 1,

        [Display(Name = "Active")]
        Active = 2,

        [Display(Name = "Completed")]
        Completed = 3,

        [Display(Name = "On Hold/Paused")]
        OnHold = 4,

        [Display(Name = "Cancelled")]
        Cancelled = 5
    }
}
