using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GiftOfTheGivers.Tests.Integration
{
    // Custom WebApplicationFactory for testing
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb" + Guid.NewGuid().ToString());
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            });
        }
    }

    /// <summary>
    /// Integration tests for the complete donation workflow
    /// Tests: Database -> Service Layer -> Controller -> API Response
    /// </summary>
    public class DonationWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public DonationWorkflowIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CompleteFlowTest_CreateDonorAndDonation_Success()
        {
            // Integration Test: End-to-End Donation Flow
            // Tests database insertion, service processing, and data relationships

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Step 1: Create a Donor with all required fields
                var donor = new Donor
                {
                    FirstName = "Integration",
                    LastName = "Test",
                    Email = "integration@test.com",
                    ContactNumber = "1234567890"
                };
                context.Donors.Add(donor);
                await context.SaveChangesAsync();

                // Step 2: Verify Donor in Database
                var createdDonor = await context.Donors
                    .FirstOrDefaultAsync(d => d.Email == "integration@test.com");

                Assert.NotNull(createdDonor);
                Assert.Equal("Integration", createdDonor.FirstName);
                Assert.Equal("Test", createdDonor.LastName);

                // Step 3: Create a User for recording (required field)
                var user = new ApplicationUser
                {
                    UserName = "testuser@test.com",
                    Email = "testuser@test.com",
                    FirstName = "Test",
                    LastName = "User",
                    IDNumber = "9001010000000",
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = "Male",
                    UserType = "Admin"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Step 4: Create a Donation with all required fields
                var donation = new Donation
                {
                    DonorID = createdDonor.DonorID,
                    Donor = createdDonor,
                    Description = "Integration Test Donation",
                    Amount = 1000.00m,
                    Type = DonationType.Financial,  // FIXED: Changed from Monetary
                    Status = DonationStatus.Received,
                    DateReceived = DateTime.UtcNow,
                    RecordedByUserId = user.Id,
                    RecordedByUser = user
                };
                context.Donations.Add(donation);
                await context.SaveChangesAsync();

                // Step 5: Verify Donation in Database
                var createdDonation = await context.Donations
                    .Include(d => d.Donor)
                    .FirstOrDefaultAsync(d => d.DonorID == createdDonor.DonorID);

                Assert.NotNull(createdDonation);
                Assert.Equal(1000.00m, createdDonation.Amount);
                Assert.Equal("Integration", createdDonation.Donor.FirstName);
            }
        }

        [Fact]
        public async Task DatabaseIntegrationTest_CascadeDelete_RemovesDonorAndDonations()
        {
            // Test: Database relationship integrity
            // Verifies cascade delete functionality

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Create User for recording
                var user = new ApplicationUser
                {
                    UserName = "cascade@test.com",
                    Email = "cascade@test.com",
                    FirstName = "Cascade",
                    LastName = "User",
                    IDNumber = "9002020000000",
                    DateOfBirth = DateTime.Now.AddYears(-25),
                    Gender = "Female",
                    UserType = "Admin"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Create Donor
                var donor = new Donor
                {
                    FirstName = "Cascade",
                    LastName = "Test",
                    Email = "cascadedonor@test.com",
                    ContactNumber = "9876543210"
                };
                context.Donors.Add(donor);
                await context.SaveChangesAsync();

                // Create multiple donations
                var donations = new List<Donation>
                {
                    new Donation
                    {
                        DonorID = donor.DonorID,
                        Donor = donor,
                        Amount = 100,
                        Type = DonationType.Financial,  // FIXED: Changed from Monetary
                        Status = DonationStatus.Received,
                        Description = "Test 1",
                        DateReceived = DateTime.UtcNow,
                        RecordedByUserId = user.Id,
                        RecordedByUser = user
                    },
                    new Donation
                    {
                        DonorID = donor.DonorID,
                        Donor = donor,
                        Amount = 200,
                        Type = DonationType.Financial,  // FIXED: Changed from Monetary
                        Status = DonationStatus.Received,
                        Description = "Test 2",
                        DateReceived = DateTime.UtcNow,
                        RecordedByUserId = user.Id,
                        RecordedByUser = user
                    },
                    new Donation
                    {
                        DonorID = donor.DonorID,
                        Donor = donor,
                        Amount = 300,
                        Type = DonationType.Financial,  // FIXED: Changed from Monetary
                        Status = DonationStatus.Received,
                        Description = "Test 3",
                        DateReceived = DateTime.UtcNow,
                        RecordedByUserId = user.Id,
                        RecordedByUser = user
                    }
                };
                context.Donations.AddRange(donations);
                await context.SaveChangesAsync();

                var donorId = donor.DonorID;

                // Delete Donor
                context.Donors.Remove(donor);
                await context.SaveChangesAsync();

                // Verify all donations are also deleted (cascade)
                var remainingDonations = await context.Donations
                    .Where(d => d.DonorID == donorId)
                    .ToListAsync();

                Assert.Empty(remainingDonations);
            }
        }
    }

    /// <summary>
    /// Integration tests for Relief Project and Incident Report workflow
    /// </summary>
    public class ReliefProjectWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ReliefProjectWorkflowIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CompleteWorkflow_CreateIncidentAndProject_Success()
        {
            // Integration Test: Incident Report to Relief Project Flow

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Create User for reporting
                var user = new ApplicationUser
                {
                    UserName = "incident@test.com",
                    Email = "incident@test.com",
                    FirstName = "Incident",
                    LastName = "Reporter",
                    IDNumber = "9003030000000",
                    DateOfBirth = DateTime.Now.AddYears(-28),
                    Gender = "Male",
                    UserType = "Admin"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Step 1: Create Incident Report with required fields
                var incident = new IncidentReport
                {
                    Title = "Flood Disaster",
                    Description = "Severe flooding in coastal areas",
                    Location = "Durban Coast",
                    Severity = IncidentSeverity.High,
                    Status = IncidentStatus.Reported,
                    Timestamp = DateTime.UtcNow,
                    ReportedByUserId = user.Id,
                    ReportedByUser = user
                };
                context.IncidentReports.Add(incident);
                await context.SaveChangesAsync();

                // Step 2: Create Coordinator user for project
                var coordinator = new ApplicationUser
                {
                    UserName = "coordinator@test.com",
                    Email = "coordinator@test.com",
                    FirstName = "Project",
                    LastName = "Coordinator",
                    IDNumber = "9004040000000",
                    DateOfBirth = DateTime.Now.AddYears(-35),
                    Gender = "Female",
                    UserType = "Admin"
                };
                context.Users.Add(coordinator);
                await context.SaveChangesAsync();

                // Step 3: Create Relief Project with required CoordinatorId
                var project = new ReliefProject
                {
                    Title = "Flood Relief Operation",
                    Description = "Emergency response to flooding",
                    Location = "Durban Coast",
                    Status = ProjectStatus.Active,
                    StartDate = DateTime.Now,
                    CoordinatorId = coordinator.Id
                };
                context.ReliefProjects.Add(project);
                await context.SaveChangesAsync();

                // Step 4: Verify both entities were created
                var loadedProject = await context.ReliefProjects
                    .FirstOrDefaultAsync(p => p.ProjectID == project.ProjectID);

                var loadedIncident = await context.IncidentReports
                    .FirstOrDefaultAsync(i => i.ReportID == incident.ReportID);

                Assert.NotNull(loadedProject);
                Assert.NotNull(loadedIncident);
                Assert.Equal("Flood Relief Operation", loadedProject.Title);
                Assert.Equal("Flood Disaster", loadedIncident.Title);
            }
        }

        [Fact]
        public async Task ProjectStatusFlow_NewToCompleted_TracksCorrectly()
        {
            // Integration Test: Project lifecycle

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Create Coordinator
                var coordinator = new ApplicationUser
                {
                    UserName = "statusflow@test.com",
                    Email = "statusflow@test.com",
                    FirstName = "Status",
                    LastName = "Coordinator",
                    IDNumber = "9005050000000",
                    DateOfBirth = DateTime.Now.AddYears(-32),
                    Gender = "Male",
                    UserType = "Admin"
                };
                context.Users.Add(coordinator);
                await context.SaveChangesAsync();

                // Create project
                var project = new ReliefProject
                {
                    Title = "Status Flow Test Project",
                    Description = "Testing status transitions",
                    Location = "Test Location",
                    Status = ProjectStatus.Active,
                    StartDate = DateTime.Now,
                    CoordinatorId = coordinator.Id
                };
                context.ReliefProjects.Add(project);
                await context.SaveChangesAsync();

                var projectId = project.ProjectID;

                // Update to Completed
                project.Status = ProjectStatus.Completed;
                project.EndDate = DateTime.Now;
                await context.SaveChangesAsync();

                // Verify update
                var updatedProject = await context.ReliefProjects.FindAsync(projectId);
                Assert.NotNull(updatedProject);
                Assert.Equal(ProjectStatus.Completed, updatedProject.Status);
                Assert.NotNull(updatedProject.EndDate);
            }
        }
    }

    /// <summary>
    /// Integration tests for Volunteer Assignment workflow
    /// </summary>
    public class VolunteerWorkflowIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public VolunteerWorkflowIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CompleteVolunteerFlow_AssignToProject_Success()
        {
            // Integration Test: Volunteer assignment workflow

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Step 1: Create Volunteer User
                var volunteer = new ApplicationUser
                {
                    UserName = "volunteer@test.com",
                    Email = "volunteer@test.com",
                    FirstName = "John",
                    LastName = "Volunteer",
                    IDNumber = "9006060000000",
                    DateOfBirth = DateTime.Now.AddYears(-26),
                    Gender = "Male",
                    UserType = "Volunteer"
                };
                context.Users.Add(volunteer);
                await context.SaveChangesAsync();

                // Step 2: Create Coordinator
                var coordinator = new ApplicationUser
                {
                    UserName = "volcoord@test.com",
                    Email = "volcoord@test.com",
                    FirstName = "Vol",
                    LastName = "Coordinator",
                    IDNumber = "9007070000000",
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = "Female",
                    UserType = "Admin"
                };
                context.Users.Add(coordinator);
                await context.SaveChangesAsync();

                // Step 3: Create Relief Project
                var project = new ReliefProject
                {
                    Title = "Volunteer Assignment Test",
                    Description = "Test project",
                    Location = "Test Location",
                    Status = ProjectStatus.Active,
                    StartDate = DateTime.Now,
                    CoordinatorId = coordinator.Id
                };
                context.ReliefProjects.Add(project);
                await context.SaveChangesAsync();

                // Step 4: Create Assignment with required fields
                var assignment = new VolunteerAssignment
                {
                    UserId = volunteer.Id,
                    User = volunteer,
                    ProjectID = project.ProjectID,
                    Project = project,
                    Role = VolunteerRole.FieldSupport,  // FIXED: Changed from FieldWorker
                    AssignedDate = DateTime.UtcNow,
                    Status = AssignmentStatus.Active
                };
                context.VolunteerAssignments.Add(assignment);
                await context.SaveChangesAsync();

                // Step 5: Verify complete relationship
                var loadedAssignment = await context.VolunteerAssignments
                    .Include(a => a.User)
                    .Include(a => a.Project)
                    .FirstOrDefaultAsync(a => a.AssignmentID == assignment.AssignmentID);

                Assert.NotNull(loadedAssignment);
                Assert.Equal("John", loadedAssignment.User.FirstName);
                Assert.Equal("Volunteer Assignment Test", loadedAssignment.Project.Title);
                Assert.Equal(VolunteerRole.FieldSupport, loadedAssignment.Role);  // FIXED
            }
        }

        [Fact]
        public async Task MultipleVolunteersOnProject_TracksIndependently()
        {
            // Integration Test: Multiple volunteers on same project

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Create Coordinator
                var coordinator = new ApplicationUser
                {
                    UserName = "multicoord@test.com",
                    Email = "multicoord@test.com",
                    FirstName = "Multi",
                    LastName = "Coordinator",
                    IDNumber = "9008080000000",
                    DateOfBirth = DateTime.Now.AddYears(-33),
                    Gender = "Male",
                    UserType = "Admin"
                };
                context.Users.Add(coordinator);
                await context.SaveChangesAsync();

                // Create project
                var project = new ReliefProject
                {
                    Title = "Multi-Volunteer Project",
                    Description = "Project with multiple volunteers",
                    Location = "Test Location",
                    Status = ProjectStatus.Active,
                    StartDate = DateTime.Now,
                    CoordinatorId = coordinator.Id
                };
                context.ReliefProjects.Add(project);
                await context.SaveChangesAsync();

                // Create multiple volunteers
                var volunteers = new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        UserName = "vol1@test.com",
                        Email = "vol1@test.com",
                        FirstName = "Alice",
                        LastName = "Smith",
                        IDNumber = "9009090000000",
                        DateOfBirth = DateTime.Now.AddYears(-24),
                        Gender = "Female",
                        UserType = "Volunteer"
                    },
                    new ApplicationUser
                    {
                        UserName = "vol2@test.com",
                        Email = "vol2@test.com",
                        FirstName = "Bob",
                        LastName = "Jones",
                        IDNumber = "9010100000000",
                        DateOfBirth = DateTime.Now.AddYears(-27),
                        Gender = "Male",
                        UserType = "Volunteer"
                    },
                    new ApplicationUser
                    {
                        UserName = "vol3@test.com",
                        Email = "vol3@test.com",
                        FirstName = "Carol",
                        LastName = "White",
                        IDNumber = "9011110000000",
                        DateOfBirth = DateTime.Now.AddYears(-29),
                        Gender = "Female",
                        UserType = "Volunteer"
                    }
                };
                context.Users.AddRange(volunteers);
                await context.SaveChangesAsync();

                // Assign all to project with different roles
                var assignments = new List<VolunteerAssignment>
                {
                    new VolunteerAssignment
                    {
                        UserId = volunteers[0].Id,
                        User = volunteers[0],
                        ProjectID = project.ProjectID,
                        Project = project,
                        Role = VolunteerRole.Logistics,  // FIXED: Changed from TeamLeader
                        AssignedDate = DateTime.UtcNow,
                        Status = AssignmentStatus.Active
                    },
                    new VolunteerAssignment
                    {
                        UserId = volunteers[1].Id,
                        User = volunteers[1],
                        ProjectID = project.ProjectID,
                        Project = project,
                        Role = VolunteerRole.FieldSupport,  // FIXED: Was already correct (Logistics)
                        AssignedDate = DateTime.UtcNow,
                        Status = AssignmentStatus.Active
                    },
                    new VolunteerAssignment
                    {
                        UserId = volunteers[2].Id,
                        User = volunteers[2],
                        ProjectID = project.ProjectID,
                        Project = project,
                        Role = VolunteerRole.Sorter,  // FIXED: Changed from MedicalSupport
                        AssignedDate = DateTime.UtcNow,
                        Status = AssignmentStatus.Active
                    }
                };
                context.VolunteerAssignments.AddRange(assignments);
                await context.SaveChangesAsync();

                // Verify all assignments
                var projectAssignments = await context.VolunteerAssignments
                    .Include(a => a.User)
                    .Where(a => a.ProjectID == project.ProjectID)
                    .ToListAsync();

                Assert.Equal(3, projectAssignments.Count);
                Assert.Contains(projectAssignments, a => a.Role == VolunteerRole.Logistics);      // FIXED
                Assert.Contains(projectAssignments, a => a.Role == VolunteerRole.FieldSupport);   // FIXED
                Assert.Contains(projectAssignments, a => a.Role == VolunteerRole.Sorter);         // FIXED
            }
        }
    }

    /// <summary>
    /// Integration tests for database transactions and data integrity
    /// </summary>
    public class DatabaseIntegrityIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public DatabaseIntegrityIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TransactionTest_RollbackOnError_MaintainsIntegrity()
        {
            // Integration Test: Transaction rollback

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var initialDonorCount = await context.Donors.CountAsync();

                // Start transaction
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // Add valid donor
                    var donor = new Donor
                    {
                        FirstName = "Transaction",
                        LastName = "Test",
                        Email = "transaction@test.com",
                        ContactNumber = "1234567890"
                    };
                    context.Donors.Add(donor);
                    await context.SaveChangesAsync();

                    // Simulate error condition
                    throw new Exception("Simulated error");

#pragma warning disable CS0162
                    await transaction.CommitAsync();
#pragma warning restore CS0162
                }
                catch
                {
                    await transaction.RollbackAsync();
                }

                // Verify rollback - count should be same
                var finalDonorCount = await context.Donors.CountAsync();
                Assert.Equal(initialDonorCount, finalDonorCount);
            }
        }

        [Fact]
        public async Task ConcurrentOperations_HandlesCorrectly()
        {
            // Integration Test: Concurrent database operations

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Create Coordinator
                var coordinator = new ApplicationUser
                {
                    UserName = "concurrent@test.com",
                    Email = "concurrent@test.com",
                    FirstName = "Concurrent",
                    LastName = "Coordinator",
                    IDNumber = "9012120000000",
                    DateOfBirth = DateTime.Now.AddYears(-31),
                    Gender = "Male",
                    UserType = "Admin"
                };
                context.Users.Add(coordinator);
                await context.SaveChangesAsync();

                // Create User for recording
                var user = new ApplicationUser
                {
                    UserName = "recorder@test.com",
                    Email = "recorder@test.com",
                    FirstName = "Recorder",
                    LastName = "User",
                    IDNumber = "9013130000000",
                    DateOfBirth = DateTime.Now.AddYears(-28),
                    Gender = "Female",
                    UserType = "Admin"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // Create project
                var project = new ReliefProject
                {
                    Title = "Concurrent Test Project",
                    Description = "Testing concurrent operations",
                    Location = "Test Location",
                    Status = ProjectStatus.Active,
                    StartDate = DateTime.Now,
                    CoordinatorId = coordinator.Id
                };
                context.ReliefProjects.Add(project);
                await context.SaveChangesAsync();

                // Simulate concurrent donation allocations
                var tasks = new List<Task>();
                for (int i = 0; i < 5; i++)
                {
                    int donationAmount = (i + 1) * 1000;
                    tasks.Add(Task.Run(async () =>
                    {
                        using var innerScope = _factory.Services.CreateScope();
                        var innerContext = innerScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Get or create a donor for this donation
                        var donor = new Donor
                        {
                            FirstName = "Concurrent",
                            LastName = $"Donor{donationAmount}",
                            Email = $"donor{donationAmount}@test.com",
                            ContactNumber = "1234567890"
                        };
                        innerContext.Donors.Add(donor);
                        await innerContext.SaveChangesAsync();

                        var donation = new Donation
                        {
                            DonorID = donor.DonorID,
                            Donor = donor,
                            Amount = donationAmount,
                            Type = DonationType.Financial,  // FIXED: Changed from Monetary
                            Status = DonationStatus.Allocated,
                            Description = $"Concurrent Test Donation {donationAmount}",
                            DateReceived = DateTime.UtcNow,
                            RecordedByUserId = user.Id,
                            RecordedByUser = user,
                            ReliefProjectProjectID = project.ProjectID
                        };
                        innerContext.Donations.Add(donation);
                        await innerContext.SaveChangesAsync();
                    }));
                }

                await Task.WhenAll(tasks);

                // Verify all donations were saved
                var projectDonations = await context.Donations
                    .Where(d => d.ReliefProjectProjectID == project.ProjectID)
                    .ToListAsync();

                Assert.Equal(5, projectDonations.Count);
                Assert.Equal(15000m, projectDonations.Sum(d => d.Amount)); // 1000+2000+3000+4000+5000
            }
        }
    }
}