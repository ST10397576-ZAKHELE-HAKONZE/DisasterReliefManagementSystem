using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models;

public class IncidentReport
{
    [Key]
    public int ReportID { get; set; } // Primary Key

    // Add 'required' to the non-nullable string properties
    public required string Title { get; set; }
    public required string Location { get; set; }
    public required string Description { get; set; }
    public required string Severity { get; set; }

    // DateTime is non-nullable but is initialized with a default value, so 'required' is NOT needed
    public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;

    public required string Status { get; set; }

    // Foreign Key (string FKs are non-nullable by default)
    public required string ReportedByUserId { get; set; }

    // Navigation property must be 'required' if non-nullable
    public required virtual ApplicationUser ReportedByUser { get; set; }
}