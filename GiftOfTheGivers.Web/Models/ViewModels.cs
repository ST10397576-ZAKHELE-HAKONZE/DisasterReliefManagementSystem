using System;
using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models.Enums; // Assuming your Enums are here

namespace GiftOfTheGivers.Web.Models.ViewModels
{
    // This model is used ONLY for receiving data from the Admin Edit form. 
    // It contains the editable fields (Status, Severity, Description) and 
    // fields needed for display (Title, ReporterEmail, Timestamp).
    public class IncidentReportEditViewModel
    {
        // Must include the ID to target the correct record in the database.
        public int ReportID { get; set; }

        // The fields that the administrator can change.
        public IncidentStatus Status { get; set; }
        public IncidentSeverity Severity { get; set; }

        // Keeps validation for the editable text field.
        [Required(ErrorMessage = "The description/notes field is required.")]
        [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
        public string Description { get; set; } = string.Empty;

        // Read-only properties needed for display in the view.
        public string Title { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}