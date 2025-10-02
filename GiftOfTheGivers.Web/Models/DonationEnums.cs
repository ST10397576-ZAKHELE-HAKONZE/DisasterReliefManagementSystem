using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models
{
    /// <summary>
    /// Defines the possible types of resources received as a donation.
    /// </summary>
    public enum DonationType
    {
        [Display(Name = "Cash/Financial")]
        Financial = 1,

        [Display(Name = "Goods/In-Kind")]
        InKind = 2,

        [Display(Name = "Service/Labor")]
        Service = 3
    }

    /// <summary>
    /// Defines the current processing status of a donation.
    /// </summary>
    public enum DonationStatus
    {
        [Display(Name = "Received")]
        Received = 1,

        [Display(Name = "Processing/Inventory")]
        Processing = 2,

        [Display(Name = "Allocated to Project")]
        Allocated = 3,

        [Display(Name = "Distributed/Used")]
        Distributed = 4
    }
}

