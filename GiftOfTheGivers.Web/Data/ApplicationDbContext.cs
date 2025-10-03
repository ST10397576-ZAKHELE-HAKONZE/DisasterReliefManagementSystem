using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// Make sure the class name matches the file name: ApplicationDbContext
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // The default constructor is fine
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

 
    public DbSet<Donor> Donors { get; set; }
    public DbSet<Donation> Donations { get; set; }
    public DbSet<ReliefProject> ReliefProjects { get; set; }
    public DbSet<VolunteerAssignment> VolunteerAssignments { get; set; }
    public DbSet<IncidentReport> IncidentReports { get; set; }

    // Configure relationships
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // FIX FOR CASCADE DELETE ERROR: Set delete behavior to NO ACTION
        builder.Entity<VolunteerAssignment>()
            .HasOne(va => va.Project)
            .WithMany(rp => rp.Assignments)
            .HasForeignKey(va => va.ProjectID)
            .OnDelete(DeleteBehavior.Restrict);

        // Inside the OnModelCreating method, after base.OnModelCreating(builder);

        // Prevent cascade delete conflict between Donor and Donation
        builder.Entity<Donation>()
            .HasOne(d => d.Donor)
            .WithMany(dr => dr.Donations)
            .HasForeignKey(d => d.DonorID)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent cascade delete conflict between ApplicationUser and Donation
        builder.Entity<Donation>()
            .HasOne(d => d.RecordedByUser)
            .WithMany() // Assuming ApplicationUser doesn't need a collection back to Donation
            .HasForeignKey(d => d.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}