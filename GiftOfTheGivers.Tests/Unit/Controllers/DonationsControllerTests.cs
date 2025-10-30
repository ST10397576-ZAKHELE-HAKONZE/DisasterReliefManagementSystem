using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace GiftOfTheGivers.Tests.Controllers
{
    public class DonationsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfDonations()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "John",
                LastName = "Smith",
                ContactNumber = "0123456789",
                Email = "john@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);

            var donation = new Donation
            {
                DonationID = 1,
                Type = DonationType.Financial,
                Amount = 1000.00m,
                Description = "Monthly donation",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Received,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Donation>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonationsController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithDonation()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "Jane",
                LastName = "Doe",
                ContactNumber = "0987654321",
                Email = "jane@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);

            var donation = new Donation
            {
                DonationID = 1,
                Type = DonationType.InKind,
                Amount = 0,
                Description = "Food supplies",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Processing,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Donation>(viewResult.Model);
            Assert.Equal(1, model.DonationID);
            Assert.Equal("Food supplies", model.Description);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonationsController(context);

            // Act
            var result = controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenModelIsValid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "Test",
                LastName = "Donor",
                ContactNumber = "0123456789",
                Email = "donor@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);
            var donation = new Donation
            {
                Type = DonationType.Financial,
                Amount = 500.00m,
                Description = "New donation",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Received,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };

            // Act
            var result = await controller.Create(donation);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Single(context.Donations);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonationsController(context);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenDonationDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonationsController(context);

            // Act
            var result = await controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithDonation()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "Test",
                LastName = "Donor",
                ContactNumber = "0123456789",
                Email = "donor@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);

            var donation = new Donation
            {
                DonationID = 1,
                Type = DonationType.Service,
                Amount = 0,
                Description = "Volunteer hours",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Allocated,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Donation>(viewResult.Model);
            Assert.Equal(1, model.DonationID);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "Test",
                LastName = "Donor",
                ContactNumber = "0123456789",
                Email = "donor@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);
            var donation = new Donation
            {
                DonationID = 1,
                Type = DonationType.Financial,
                Amount = 1000.00m,
                Description = "Updated",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Distributed,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };

            // Act
            var result = await controller.Edit(2, donation);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_UpdatesDonation_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "Test",
                LastName = "Donor",
                ContactNumber = "0123456789",
                Email = "donor@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);

            var donation = new Donation
            {
                DonationID = 1,
                Type = DonationType.Financial,
                Amount = 500.00m,
                Description = "Original",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Received,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);
            donation.Description = "Updated description";
            donation.Status = DonationStatus.Allocated;

            // Act
            var result = await controller.Edit(1, donation);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedDonation = await context.Donations.FindAsync(1);
            Assert.Equal("Updated description", updatedDonation.Description);
            Assert.Equal(DonationStatus.Allocated, updatedDonation.Status);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonationsController(context);

            // Act
            var result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesDonation_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "Test",
                LastName = "Donor",
                ContactNumber = "0123456789",
                Email = "donor@example.com"
            };
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "staff@example.com",
                UserName = "staff@example.com",
                FirstName = "Staff",
                LastName = "Member",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Donors.Add(donor);
            context.Users.Add(user);

            var donation = new Donation
            {
                DonationID = 1,
                Type = DonationType.Financial,
                Amount = 500.00m,
                Description = "To be deleted",
                DateReceived = System.DateTime.UtcNow,
                Status = DonationStatus.Received,
                DonorID = 1,
                Donor = donor,
                RecordedByUserId = user.Id,
                RecordedByUser = user
            };
            context.Donations.Add(donation);
            await context.SaveChangesAsync();

            var controller = new DonationsController(context);


            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var deletedDonation = await context.Donations.FindAsync(1);
            Assert.Null(deletedDonation);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex_WhenDonationNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonationsController(context);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}