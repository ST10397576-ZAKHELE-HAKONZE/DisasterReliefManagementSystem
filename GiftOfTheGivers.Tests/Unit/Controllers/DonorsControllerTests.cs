using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class DonorsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfDonors()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor1 = new Donor
            {
                DonorID = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ContactNumber = "0821234567"
            };
            var donor2 = new Donor
            {
                DonorID = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                ContactNumber = "0839876543"
            };
            context.Donors.AddRange(donor1, donor2);
            await context.SaveChangesAsync();

            var controller = new DonorsController(context);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Donor>>(result.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonorsController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithDonor()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ContactNumber = "0821234567"
            };
            context.Donors.Add(donor);
            await context.SaveChangesAsync();

            var controller = new DonorsController(context);

            // Act
            var result = await controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<Donor>(result.Model);
            Assert.Equal("John", model.FirstName);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonorsController(context);

            // Act
            var result = controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_Post_ValidModel_AddsDonorAndRedirects()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new DonorsController(context);
            var newDonor = new Donor
            {
                DonorID = 3,
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice@example.com",
                ContactNumber = "0841112222"
            };

            // Act
            var result = await controller.Create(newDonor) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Single(context.Donors);
        }

        [Fact]
        public async Task Edit_Get_ExistingId_ReturnsViewWithDonor()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ContactNumber = "0821234567"
            };
            context.Donors.Add(donor);
            await context.SaveChangesAsync();

            var controller = new DonorsController(context);

            // Act
            var result = await controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<Donor>(result.Model);
            Assert.Equal("John", model.FirstName);
        }

        [Fact]
        public async Task Edit_Post_ValidIdAndModel_UpdatesAndSaves()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ContactNumber = "0821234567"
            };
            context.Donors.Add(donor);
            await context.SaveChangesAsync();

            // CRITICAL FIX: Detach the entity to avoid tracking conflict
            context.Entry(donor).State = EntityState.Detached;

            var controller = new DonorsController(context);
            var updatedDonor = new Donor
            {
                DonorID = 1,
                FirstName = "Johnny",
                LastName = "Doe",
                Email = "john@example.com",
                ContactNumber = "0821234567"
            };

            // Act
            var result = await controller.Edit(1, updatedDonor) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            var saved = await context.Donors.FindAsync(1);
            Assert.Equal("Johnny", saved.FirstName);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesDonorAndSavesChanges()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var donor = new Donor
            {
                DonorID = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                ContactNumber = "0821234567"
            };
            context.Donors.Add(donor);
            await context.SaveChangesAsync();

            var controller = new DonorsController(context);

            // Act
            await controller.DeleteConfirmed(1);

            // Assert
            var deleted = await context.Donors.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}