using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    // Represents a report submitted by a user detailing a disaster or emergency incident.
    public class IncidentReport
    {
        [Key]
        public int ReportID { get; set; } // Primary Key

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Location { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public required IncidentSeverity Severity { get; set; }

        // DateTime is non-nullable but is initialized with a default value, so 'required' is NOT needed
        public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;

        [Required]
        public required IncidentStatus Status { get; set; }

        // Foreign Key
        [Required]
        public required string ReportedByUserId { get; set; }

        // Navigation property
        public required virtual ApplicationUser ReportedByUser { get; set; } = default!;
    }
}
