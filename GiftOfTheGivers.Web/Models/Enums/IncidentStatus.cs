using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models.Enums
{
    
    public enum IncidentStatus
    {
        [Display(Name = "Reported (New)")]
        Reported = 1,

        [Display(Name = "Under Investigation")]
        Investigating = 2,

        [Display(Name = "Relief Operations Active")]
        ActiveRelief = 3,

        [Display(Name = "Resolved / Closed")]
        Resolved = 4,

        [Display(Name = "False Report")]
        False = 5
    }
}
