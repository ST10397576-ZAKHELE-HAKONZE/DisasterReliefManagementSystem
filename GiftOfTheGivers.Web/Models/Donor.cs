using GiftOfTheGivers.Web.Models;
using System.Collections.Generic;

public class Donor
{
    public int DonorID { get; set; } // Primary Key
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

    public ICollection<Donation> Donations { get; set; }
}