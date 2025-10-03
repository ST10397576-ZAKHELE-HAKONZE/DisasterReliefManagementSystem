using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models.Enums 
{
    
    public enum IncidentSeverity
    {
        [Display(Name = "Minor Impact")]
        Minor = 1, // Used to be 'Low'

        [Display(Name = "Moderate Impact")]
        Moderate = 2, // Used to be 'Medium'

        [Display(Name = "High Impact")]
        High = 3,

        [Display(Name = "Critical Emergency")]
        Critical = 4
    }
}