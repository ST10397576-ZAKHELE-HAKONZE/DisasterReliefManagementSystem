using GiftOfTheGivers.Web.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Donation
{
    public int DonationID { get; set; } // Primary Key
    public string ItemType { get; set; }
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ValueZAR { get; set; }
    public System.DateTime DonationDate { get; set; } = System.DateTime.UtcNow;
    public string Status { get; set; }

    // Foreign Keys (IdentityUser PK is string)
    public int? DonorID { get; set; } // FK to Donors (Nullable: if logged-in user donates)
    public virtual Donor Donor { get; set; }

    public string? UserId { get; set; } // FK to ApplicationUser (Nullable: if anonymous donor)
    public virtual ApplicationUser User { get; set; }

    public int ProjectID { get; set; } // FK to ReliefProject (Must link donation to a project)
    public virtual ReliefProject Project { get; set; }
}
