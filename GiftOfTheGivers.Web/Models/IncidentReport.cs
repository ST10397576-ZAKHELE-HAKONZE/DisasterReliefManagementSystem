using System.ComponentModel.DataAnnotations;

public class IncidentReport
{
    [Key]
    public int ReportID { get; set; } // Primary Key
    public string Title { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }
    public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;
    public string Status { get; set; }

    // Foreign Key
    public string ReportedByUserId { get; set; }
    public virtual ApplicationUser ReportedByUser { get; set; }
}