using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.ViewModels;
using GiftOfTheGivers.Web.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiftOfTheGivers.Tests.Controllers
{
    /// <summary>
    /// Unit tests for AdminController
    /// Test Coverage: CRUD operations, authorization, data validation
    /// </summary>
    public class DonationWorkflowIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DonationWorkflowIntegrationTests()
        {
            // Use InMemory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestAdminDatabase_{System.Guid.NewGuid()}")
                .Options;
        }

        private ApplicationDbContext GetInMemoryContext()
        {
            var context = new ApplicationDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            return context;
        }

        private AdminController CreateControllerWithTempData(ApplicationDbContext context)
        {
            var controller = new AdminController(context);

            // Mock TempData
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewWithAllReports()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<IncidentReport>>(viewResult.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task Index_DisplaysSuccessMessage_FromTempData()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);
            controller.TempData["SuccessMessage"] = "Test success message";

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Test success message", viewResult.ViewData["SuccessMessage"]);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewModelWithCorrectData()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IncidentReportEditViewModel>(viewResult.Model);
            Assert.Equal(1, model.ReportID);
            Assert.Equal("Flood Emergency", model.Title);
            Assert.Equal("test@example.com", model.ReporterEmail);
        }

        [Fact]
        public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WithNullId_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_UpdatesReportSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            var viewModel = new IncidentReportEditViewModel
            {
                ReportID = 1,
                Status = IncidentStatus.ActiveRelief,
                Severity = IncidentSeverity.Critical,
                Description = "Updated description"
            };

            // Act
            var result = await controller.Edit(1, viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify TempData message was set
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Contains("successfully updated", controller.TempData["SuccessMessage"].ToString());

            // Verify database was updated
            var updatedReport = await context.IncidentReports.FindAsync(1);
            Assert.Equal(IncidentStatus.ActiveRelief, updatedReport.Status);
            Assert.Equal("Updated description", updatedReport.Description);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);
            controller.ModelState.AddModelError("Description", "Required");

            var viewModel = new IncidentReportEditViewModel
            {
                ReportID = 1,
                Status = IncidentStatus.Investigating,
                Severity = IncidentSeverity.High,
                Description = "" // Invalid - empty
            };

            // Act
            var result = await controller.Edit(1, viewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            var model = Assert.IsType<IncidentReportEditViewModel>(viewResult.Model);

            // Verify read-only fields were repopulated
            Assert.Equal("Flood Emergency", model.Title);
            Assert.Equal("test@example.com", model.ReporterEmail);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewWithReport()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IncidentReport>(viewResult.Model);
            Assert.Equal(1, model.ReportID);
        }

        [Fact]
        public async Task Delete_Get_WithNullId_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesReportFromDatabase()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify TempData message was set
            Assert.True(controller.TempData.ContainsKey("SuccessMessage"));
            Assert.Contains("successfully deleted", controller.TempData["SuccessMessage"].ToString());

            // Verify deletion
            var deletedReport = await context.IncidentReports.FindAsync(1);
            Assert.Null(deletedReport);
        }

        [Fact]
        public async Task DeleteConfirmed_HandlesNonExistentRecord()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithTempData(context);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_Post_WithMismatchedId_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            var viewModel = new IncidentReportEditViewModel
            {
                ReportID = 2, // Mismatched
                Status = IncidentStatus.Investigating,
                Severity = IncidentSeverity.Moderate,
                Description = "Test"
            };

            // Act
            var result = await controller.Edit(1, viewModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_PreservesReadOnlyFields()
        {
            // Arrange
            using var context = GetInMemoryContext();
            SeedTestData(context);
            var controller = CreateControllerWithTempData(context);

            var originalReport = await context.IncidentReports
                .Include(i => i.ReportedByUser)
                .FirstAsync(i => i.ReportID == 1);

            var originalTimestamp = originalReport.Timestamp;
            var originalUserId = originalReport.ReportedByUserId;

            var viewModel = new IncidentReportEditViewModel
            {
                ReportID = 1,
                Status = IncidentStatus.Resolved,
                Severity = IncidentSeverity.Minor,
                Description = "Updated by admin"
            };

            // Act
            await controller.Edit(1, viewModel);

            // Assert
            var updatedReport = await context.IncidentReports.FindAsync(1);
            Assert.Equal(originalTimestamp, updatedReport.Timestamp);
            Assert.Equal(originalUserId, updatedReport.ReportedByUserId);
            Assert.Equal("Updated by admin", updatedReport.Description);
        }

        [Fact]
        public async Task Edit_Post_HandlesConcurrencyException()
        {
            // Arrange
            using var context1 = GetInMemoryContext();
            using var context2 = new ApplicationDbContext(_options);

            SeedTestData(context1);

            var controller = CreateControllerWithTempData(context2);

            // Delete the record in context1
            var reportToDelete = await context1.IncidentReports.FindAsync(1);
            context1.IncidentReports.Remove(reportToDelete);
            await context1.SaveChangesAsync();

            var viewModel = new IncidentReportEditViewModel
            {
                ReportID = 1,
                Status = IncidentStatus.Investigating,
                Severity = IncidentSeverity.High,
                Description = "Test"
            };

            // Act
            var result = await controller.Edit(1, viewModel);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = new System.DateTime(1990, 1, 1),
                Gender = "Male",
                UserType = "Volunteer"
            };

            context.Users.Add(user);

            var reports = new List<IncidentReport>
            {
                new IncidentReport
                {
                    ReportID = 1,
                    Title = "Flood Emergency",
                    Location = "Durban",
                    Description = "Severe flooding in residential area",
                    Severity = IncidentSeverity.Critical,
                    Status = IncidentStatus.Reported,
                    ReportedByUserId = "user1",
                    ReportedByUser = user,
                    Timestamp = System.DateTime.UtcNow
                },
                new IncidentReport
                {
                    ReportID = 2,
                    Title = "Fire Incident",
                    Location = "Cape Town",
                    Description = "Building fire reported",
                    Severity = IncidentSeverity.High,
                    Status = IncidentStatus.Investigating,
                    ReportedByUserId = "user1",
                    ReportedByUser = user,
                    Timestamp = System.DateTime.UtcNow
                },
                new IncidentReport
                {
                    ReportID = 3,
                    Title = "Road Damage",
                    Location = "Johannesburg",
                    Description = "Major pothole causing accidents",
                    Severity = IncidentSeverity.Moderate,
                    Status = IncidentStatus.Resolved,
                    ReportedByUserId = "user1",
                    ReportedByUser = user,
                    Timestamp = System.DateTime.UtcNow
                }
            };

            context.IncidentReports.AddRange(reports);
            context.SaveChanges();
        }
    }
}