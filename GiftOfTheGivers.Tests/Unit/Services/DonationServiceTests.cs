using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.Enums;
using GiftOfTheGivers.Web.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DonationStatus = GiftOfTheGivers.Web.Models.DonationStatus;
using DonationType = GiftOfTheGivers.Web.Models.DonationType;

namespace GiftOfTheGivers.Tests.Services
{
    /// <summary>
    /// Comprehensive unit tests for DonationService
    /// Test Coverage: CRUD operations, filtering, edge cases, error handling
    /// </summary>
    public class DonationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DonationService _service;

        public DonationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new DonationService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Helper Methods

        private ApplicationUser CreateTestUser(string id, string email)
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = email,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
        }

        private Donor CreateTestDonor(int id, string firstName, string lastName)
        {
            return new Donor
            {
                DonorID = id,
                FirstName = firstName,
                LastName = lastName,
                ContactNumber = "0123456789",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com"
            };
        }

        private ReliefProject CreateTestProject(int id, string title, ApplicationUser coordinator)
        {
            return new ReliefProject
            {
                ProjectID = id,
                Title = title,
                Location = "Test Location",
                Description = "Test Description",
                Status = ProjectStatus.Active,
                StartDate = DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
        }

        private Donation CreateTestDonation(
            int id,
            DonationType type,
            decimal amount,
            string description,
            DonationStatus status,
            Donor donor,
            ApplicationUser recordedBy,
            DateTime? dateReceived = null,
            ReliefProject? project = null)
        {
            return new Donation
            {
                DonationID = id,
                Type = type,
                Amount = amount,
                Description = description,
                DateReceived = dateReceived ?? DateTime.UtcNow,
                Status = status,
                DonorID = donor.DonorID,
                Donor = donor,
                RecordedByUserId = recordedBy.Id,
                RecordedByUser = recordedBy,
                ReliefProjectProjectID = project?.ProjectID,
                ReliefProject = project
            };
        }

        private async Task SeedTestData()
        {
            var user1 = CreateTestUser("user1", "staff1@example.com");
            var user2 = CreateTestUser("user2", "staff2@example.com");
            var coordinator = CreateTestUser("coord1", "coordinator@example.com");

            var donor1 = CreateTestDonor(1, "John", "Smith");
            var donor2 = CreateTestDonor(2, "Jane", "Doe");
            var donor3 = CreateTestDonor(3, "Bob", "Johnson");

            var project1 = CreateTestProject(1, "Flood Relief", coordinator);

            _context.Users.AddRange(user1, user2, coordinator);
            _context.Donors.AddRange(donor1, donor2, donor3);
            _context.ReliefProjects.Add(project1);
            await _context.SaveChangesAsync();

            // Add donations with different dates and statuses
            var donations = new List<Donation>
            {
                CreateTestDonation(1, DonationType.Financial, 1000m, "Monthly donation",
                    DonationStatus.Received, donor1, user1, DateTime.UtcNow.AddDays(-5)),

                CreateTestDonation(2, DonationType.InKind, 0m, "Food supplies",
                    DonationStatus.Received, donor2, user1, DateTime.UtcNow.AddDays(-3)),

                CreateTestDonation(3, DonationType.Financial, 500m, "Emergency fund",
                    DonationStatus.Received, donor1, user2, DateTime.UtcNow.AddDays(-1)),

                CreateTestDonation(4, DonationType.Service, 0m, "Volunteer hours",
                    DonationStatus.Processing, donor3, user2, DateTime.UtcNow.AddDays(-10)),

                CreateTestDonation(5, DonationType.Financial, 2000m, "Annual contribution",
                    DonationStatus.Received, donor2, user1, DateTime.UtcNow, project1),

                CreateTestDonation(6, DonationType.InKind, 0m, "Medical supplies",
                    DonationStatus.Allocated, donor3, user2, DateTime.UtcNow.AddDays(-7))
            };

            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region GetRecentDonationsAsync Tests

        [Fact]
        public async Task GetRecentDonationsAsync_ReturnsDefaultFiveDonations_WhenNoCountSpecified()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetRecentDonationsAsync();

            // Assert
            Assert.NotNull(result);
            // Only 4 donations have "Received" status in our test data
            Assert.True(result.Count <= 5, "Should return at most 5 donations");
            Assert.Equal(4, result.Count); // Actual count of "Received" donations
        }

        [Fact]
        public async Task GetRecentDonationsAsync_ReturnsOnlyReceivedStatus()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetRecentDonationsAsync();

            // Assert
            Assert.All(result, donation =>
                Assert.Equal(DonationStatus.Received, donation.Status));
        }

        [Fact]
        public async Task GetRecentDonationsAsync_ReturnsInDescendingOrderByDate()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetRecentDonationsAsync();

            // Assert
            // The service only returns donations with Status = "Received"
            // From your seed data: donations 1, 2, 3, 5 have "Received" status
            // Donation 4 has "Processing" and donation 6 has "Allocated" - both excluded
            Assert.Equal(4, result.Count); // Only 4 donations have "Received" status

            // Verify they are in descending order by DateReceived
            Assert.Equal(5, result[0].DonationID); // Most recent (UtcNow)
            Assert.Equal(3, result[1].DonationID); // UtcNow - 1 day
            Assert.Equal(2, result[2].DonationID); // UtcNow - 3 days
            Assert.Equal(1, result[3].DonationID); // UtcNow - 5 days
                                                   // Note: Donations 4 and 6 are excluded due to their status
        }

        [Fact]
        public async Task GetRecentDonationsAsync_ReturnsRequestedCount_WhenCountSpecified()
        {
            // Arrange
            await SeedTestData();
            var requestedCount = 3;

            // Act
            var result = await _service.GetRecentDonationsAsync(requestedCount);

            // Assert
            Assert.Equal(requestedCount, result.Count);
        }

        [Fact]
        public async Task GetRecentDonationsAsync_IncludesDonorInformation()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetRecentDonationsAsync();

            // Assert
            Assert.All(result, donation =>
            {
                Assert.NotNull(donation.Donor);
                Assert.NotNull(donation.Donor.FirstName);
                Assert.NotNull(donation.Donor.LastName);
            });
        }

        [Fact]
        public async Task GetRecentDonationsAsync_IncludesReliefProjectWhenAssigned()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetRecentDonationsAsync();

            // Assert
            var donationWithProject = result.FirstOrDefault(d => d.ReliefProjectProjectID != null);
            Assert.NotNull(donationWithProject);
            Assert.NotNull(donationWithProject.ReliefProject);
        }

        [Fact]
        public async Task GetRecentDonationsAsync_ReturnsEmptyList_WhenNoDonationsExist()
        {
            // Arrange - no data seeded

            // Act
            var result = await _service.GetRecentDonationsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRecentDonationsAsync_ReturnsLessThanRequested_WhenNotEnoughDonations()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var donation = CreateTestDonation(1, DonationType.Financial, 100m,
                "Single donation", DonationStatus.Received, donor, user);
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetRecentDonationsAsync(10);

            // Assert
            Assert.Single(result);
        }

        #endregion

        #region GetDonationByIdAsync Tests

        [Fact]
        public async Task GetDonationByIdAsync_ReturnsCorrectDonation_WhenIdExists()
        {
            // Arrange
            await SeedTestData();
            var expectedId = 1;

            // Act
            var result = await _service.GetDonationByIdAsync(expectedId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedId, result.DonationID);
        }

        [Fact]
        public async Task GetDonationByIdAsync_ReturnsDonationWithDonor()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetDonationByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Donor);
            Assert.Equal("John", result.Donor.FirstName);
        }

        [Fact]
        public async Task GetDonationByIdAsync_ReturnsDonationWithReliefProject_WhenAssigned()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetDonationByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ReliefProject);
            Assert.Equal("Flood Relief", result.ReliefProject.Title);
        }

        [Fact]
        public async Task GetDonationByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetDonationByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDonationByIdAsync_ReturnsNull_WhenDatabaseIsEmpty()
        {
            // Arrange - no data seeded

            // Act
            var result = await _service.GetDonationByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDonationByIdAsync_HandlesNegativeId()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetDonationByIdAsync(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDonationByIdAsync_HandlesZeroId()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _service.GetDonationByIdAsync(0);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddDonationAsync Tests

        [Fact]
        public async Task AddDonationAsync_SuccessfullyAddsDonation()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var newDonation = CreateTestDonation(0, DonationType.Financial, 750m,
                "New donation", DonationStatus.Received, donor, user);

            // Act
            await _service.AddDonationAsync(newDonation);

            // Assert
            var savedDonation = await _context.Donations
                .FirstOrDefaultAsync(d => d.Description == "New donation");
            Assert.NotNull(savedDonation);
            Assert.Equal(750m, savedDonation.Amount);
        }

        [Fact]
        public async Task AddDonationAsync_AssignsCorrectId()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var newDonation = CreateTestDonation(0, DonationType.InKind, 0m,
                "Test donation", DonationStatus.Received, donor, user);

            // Act
            await _service.AddDonationAsync(newDonation);

            // Assert
            var savedDonation = await _context.Donations
                .FirstOrDefaultAsync(d => d.Description == "Test donation");
            Assert.NotNull(savedDonation);
            Assert.True(savedDonation.DonationID > 0);
        }

        [Fact]
        public async Task AddDonationAsync_IncreasesTotalDonationCount()
        {
            // Arrange
            await SeedTestData();
            var initialCount = await _context.Donations.CountAsync();

            var user = await _context.Users.FirstAsync();
            var donor = await _context.Donors.FirstAsync();
            var newDonation = CreateTestDonation(0, DonationType.Financial, 100m,
                "Additional donation", DonationStatus.Received, donor, user);

            // Act
            await _service.AddDonationAsync(newDonation);

            // Assert
            var finalCount = await _context.Donations.CountAsync();
            Assert.Equal(initialCount + 1, finalCount);
        }

        [Fact]
        public async Task AddDonationAsync_PreservesAllProperties()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            var coordinator = CreateTestUser("coord1", "coordinator@example.com");
            var project = CreateTestProject(1, "Test Project", coordinator);

            _context.Users.AddRange(user, coordinator);
            _context.Donors.Add(donor);
            _context.ReliefProjects.Add(project);
            await _context.SaveChangesAsync();

            var testDate = DateTime.UtcNow.AddDays(-2);
            var newDonation = CreateTestDonation(0, DonationType.Service, 0m,
                "Volunteer work", DonationStatus.Allocated, donor, user, testDate, project);

            // Act
            await _service.AddDonationAsync(newDonation);

            // Assert
            var savedDonation = await _context.Donations
                .Include(d => d.ReliefProject)
                .FirstOrDefaultAsync(d => d.Description == "Volunteer work");

            Assert.NotNull(savedDonation);
            Assert.Equal(DonationType.Service, savedDonation.Type);
            Assert.Equal(DonationStatus.Allocated, savedDonation.Status);
            Assert.Equal(testDate.Date, savedDonation.DateReceived.Date);
            Assert.Equal(project.ProjectID, savedDonation.ReliefProjectProjectID);
        }

        [Fact]
        public async Task AddDonationAsync_HandlesMultipleConsecutiveAdds()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            // Act
            for (int i = 0; i < 5; i++)
            {
                var donation = CreateTestDonation(0, DonationType.Financial, 100m * i,
                    $"Donation {i}", DonationStatus.Received, donor, user);
                await _service.AddDonationAsync(donation);
            }

            // Assert
            var count = await _context.Donations.CountAsync();
            Assert.Equal(5, count);
        }

        #endregion

        #region UpdateDonationAsync Tests

        [Fact]
        public async Task UpdateDonationAsync_SuccessfullyUpdatesExistingDonation()
        {
            // Arrange
            await SeedTestData();
            var donationToUpdate = await _context.Donations.FindAsync(1);
            Assert.NotNull(donationToUpdate);

            donationToUpdate.Amount = 1500m;
            donationToUpdate.Description = "Updated monthly donation";
            donationToUpdate.Status = DonationStatus.Allocated;

            // Act
            await _service.UpdateDonationAsync(donationToUpdate);

            // Assert
            var updatedDonation = await _context.Donations.FindAsync(1);
            Assert.NotNull(updatedDonation);
            Assert.Equal(1500m, updatedDonation.Amount);
            Assert.Equal("Updated monthly donation", updatedDonation.Description);
            Assert.Equal(DonationStatus.Allocated, updatedDonation.Status);
        }

        [Fact]
        public async Task UpdateDonationAsync_UpdatesOnlySpecifiedProperties()
        {
            // Arrange
            await SeedTestData();
            var originalDonation = await _context.Donations
                .AsNoTracking()
                .FirstAsync(d => d.DonationID == 1);

            var donationToUpdate = await _context.Donations.FindAsync(1);
            Assert.NotNull(donationToUpdate);

            // Only update status
            donationToUpdate.Status = DonationStatus.Distributed;

            // Act
            await _service.UpdateDonationAsync(donationToUpdate);

            // Assert
            var updatedDonation = await _context.Donations.FindAsync(1);
            Assert.NotNull(updatedDonation);
            Assert.Equal(DonationStatus.Distributed, updatedDonation.Status);
            Assert.Equal(originalDonation.Amount, updatedDonation.Amount);
            Assert.Equal(originalDonation.Description, updatedDonation.Description);
        }

        [Fact]
        public async Task UpdateDonationAsync_CanAssignToReliefProject()
        {
            // Arrange
            await SeedTestData();
            var donation = await _context.Donations.FindAsync(1);
            var project = await _context.ReliefProjects.FirstAsync();
            Assert.NotNull(donation);

            donation.ReliefProjectProjectID = project.ProjectID;
            donation.Status = DonationStatus.Allocated;

            // Act
            await _service.UpdateDonationAsync(donation);

            // Assert
            var updatedDonation = await _context.Donations
                .Include(d => d.ReliefProject)
                .FirstAsync(d => d.DonationID == 1);

            Assert.Equal(project.ProjectID, updatedDonation.ReliefProjectProjectID);
            Assert.NotNull(updatedDonation.ReliefProject);
        }

        [Fact]
        public async Task UpdateDonationAsync_DoesNotAffectOtherDonations()
        {
            // Arrange
            await SeedTestData();
            var initialCount = await _context.Donations.CountAsync();

            var donationToUpdate = await _context.Donations.FindAsync(1);
            Assert.NotNull(donationToUpdate);
            donationToUpdate.Amount = 9999m;

            // Act
            await _service.UpdateDonationAsync(donationToUpdate);

            // Assert
            var finalCount = await _context.Donations.CountAsync();
            Assert.Equal(initialCount, finalCount);

            var otherDonation = await _context.Donations.FindAsync(2);
            Assert.NotNull(otherDonation);
            Assert.NotEqual(9999m, otherDonation.Amount);
        }

        #endregion

        #region DeleteDonationAsync Tests

        [Fact]
        public async Task DeleteDonationAsync_SuccessfullyRemovesDonation()
        {
            // Arrange
            await SeedTestData();
            var donationId = 1;

            // Act
            await _service.DeleteDonationAsync(donationId);

            // Assert
            var deletedDonation = await _context.Donations.FindAsync(donationId);
            Assert.Null(deletedDonation);
        }

        [Fact]
        public async Task DeleteDonationAsync_DecreasesTotalCount()
        {
            // Arrange
            await SeedTestData();
            var initialCount = await _context.Donations.CountAsync();

            // Act
            await _service.DeleteDonationAsync(1);

            // Assert
            var finalCount = await _context.Donations.CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        }

        [Fact]
        public async Task DeleteDonationAsync_DoesNotThrowException_WhenIdDoesNotExist()
        {
            // Arrange
            await SeedTestData();

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
                await _service.DeleteDonationAsync(999));

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteDonationAsync_DoesNotAffectOtherDonations()
        {
            // Arrange
            await SeedTestData();
            var donationsBeforeDelete = await _context.Donations
                .Where(d => d.DonationID != 1)
                .ToListAsync();

            // Act
            await _service.DeleteDonationAsync(1);

            // Assert
            foreach (var donation in donationsBeforeDelete)
            {
                var stillExists = await _context.Donations.FindAsync(donation.DonationID);
                Assert.NotNull(stillExists);
            }
        }

        [Fact]
        public async Task DeleteDonationAsync_HandlesMultipleConsecutiveDeletes()
        {
            // Arrange
            await SeedTestData();
            var initialCount = await _context.Donations.CountAsync();

            // Act
            await _service.DeleteDonationAsync(1);
            await _service.DeleteDonationAsync(2);
            await _service.DeleteDonationAsync(3);

            // Assert
            var finalCount = await _context.Donations.CountAsync();
            Assert.Equal(initialCount - 3, finalCount);
        }

        [Fact]
        public async Task DeleteDonationAsync_HandlesZeroId()
        {
            // Arrange
            await SeedTestData();

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
                await _service.DeleteDonationAsync(0));

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteDonationAsync_HandlesNegativeId()
        {
            // Arrange
            await SeedTestData();

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
                await _service.DeleteDonationAsync(-1));

            Assert.Null(exception);
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public async Task GetRecentDonationsAsync_PerformanceTest_LargeDataset()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            // Add 100 donations
            for (int i = 0; i < 100; i++)
            {
                var donation = CreateTestDonation(0, DonationType.Financial, 100m,
                    $"Donation {i}", DonationStatus.Received, donor, user,
                    DateTime.UtcNow.AddDays(-i));
                _context.Donations.Add(donation);
            }
            await _context.SaveChangesAsync();

            // Act
            var startTime = DateTime.UtcNow;
            var result = await _service.GetRecentDonationsAsync(10);
            var endTime = DateTime.UtcNow;

            // Assert
            Assert.Equal(10, result.Count);
            Assert.True((endTime - startTime).TotalSeconds < 2,
                "Query should complete in under 2 seconds");
        }

        [Fact]
        public async Task Service_HandlesAllDonationTypes()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var donationTypes = Enum.GetValues(typeof(DonationType)).Cast<DonationType>();

            // Act & Assert
            foreach (var type in donationTypes)
            {
                var donation = CreateTestDonation(0, type, 100m,
                    $"{type} donation", DonationStatus.Received, donor, user);

                await _service.AddDonationAsync(donation);

                var saved = await _context.Donations
                    .FirstOrDefaultAsync(d => d.Description == $"{type} donation");

                Assert.NotNull(saved);
                Assert.Equal(type, saved.Type);
            }
        }

        [Fact]
        public async Task Service_HandlesAllDonationStatuses()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var donationStatuses = Enum.GetValues(typeof(DonationStatus)).Cast<DonationStatus>();

            // Act & Assert
            foreach (var status in donationStatuses)
            {
                var donation = CreateTestDonation(0, DonationType.Financial, 100m,
                    $"{status} donation", status, donor, user);

                await _service.AddDonationAsync(donation);

                var saved = await _context.Donations
                    .FirstOrDefaultAsync(d => d.Description == $"{status} donation");

                Assert.NotNull(saved);
                Assert.Equal(status, saved.Status);
            }
        }

        [Fact]
        public async Task Service_HandlesZeroAmountDonations()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var donation = CreateTestDonation(0, DonationType.InKind, 0m,
                "Non-monetary donation", DonationStatus.Received, donor, user);

            // Act
            await _service.AddDonationAsync(donation);

            // Assert
            var saved = await _context.Donations
                .FirstOrDefaultAsync(d => d.Description == "Non-monetary donation");
            Assert.NotNull(saved);
            Assert.Equal(0m, saved.Amount);
        }

        [Fact]
        public async Task Service_HandlesLargeAmountDonations()
        {
            // Arrange
            var user = CreateTestUser("user1", "staff@example.com");
            var donor = CreateTestDonor(1, "Test", "Donor");
            _context.Users.Add(user);
            _context.Donors.Add(donor);
            await _context.SaveChangesAsync();

            var largeAmount = 999999999.99m;
            var donation = CreateTestDonation(0, DonationType.Financial, largeAmount,
                "Large donation", DonationStatus.Received, donor, user);

            // Act
            await _service.AddDonationAsync(donation);

            // Assert
            var saved = await _context.Donations
                .FirstOrDefaultAsync(d => d.Description == "Large donation");
            Assert.NotNull(saved);
            Assert.Equal(largeAmount, saved.Amount);
        }

        #endregion
    }
}