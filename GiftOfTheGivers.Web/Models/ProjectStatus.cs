using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    public enum ProjectStatus
    {
        [Display(Name = "Planning")]
        Planning = 1,

        [Display(Name = "Active")]
        Active = 2,

        [Display(Name = "On Hold")]
        OnHold = 3,

        [Display(Name = "Completed")]
        Completed = 4
    }
}
