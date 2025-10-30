using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;           // ADD THIS
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Models.Enums;
using System.Threading.Tasks;
using System.Security.Claims;              // ADD THIS

namespace GiftOfTheGivers.Tests.Controllers
{
    public class IncidentReportsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithUserReports()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);

            var report = new IncidentReport
            {
                ReportID = 1,
                Title = "Flood",
                Location = "Cape Town",
                Description = "Severe flooding",
                Severity = IncidentSeverity.High,
                Status = IncidentStatus.Reported,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = System.DateTime.UtcNow
            };
            context.IncidentReports.Add(report);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // FIX: Add HttpContext with User
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user1"),
                new Claim(ClaimTypes.Name, "test@example.com")
            }));
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<IncidentReport>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Index_RedirectsToLogin_WhenUserIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // FIX: Add HttpContext
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Account/Login", redirectResult.PageName);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithReport()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);

            var report = new IncidentReport
            {
                ReportID = 1,
                Title = "Fire",
                Location = "Johannesburg",
                Description = "Building fire",
                Severity = IncidentSeverity.Critical,
                Status = IncidentStatus.ActiveRelief,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = System.DateTime.UtcNow
            };
            context.IncidentReports.Add(report);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // FIX: Add HttpContext
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IncidentReport>(viewResult.Model);
            Assert.Equal(1, model.ReportID);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

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
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // FIX: Add HttpContext
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var report = new IncidentReport
            {
                Title = "New Report",
                Location = "Durban",
                Description = "Emergency situation",
                Severity = IncidentSeverity.Moderate,
                Status = IncidentStatus.Reported,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            var result = await controller.Create(report);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Single(context.IncidentReports);
        }

        [Fact]
        public async Task Create_Post_RedirectsToLogin_WhenUserIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // FIX: Add HttpContext
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var report = new IncidentReport
            {
                Title = "Test",
                Location = "Test",
                Description = "Test",
                Severity = IncidentSeverity.Minor,
                Status = IncidentStatus.Reported,
                ReportedByUserId = "user1",
                ReportedByUser = null,
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            var result = await controller.Create(report);

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/Account/Login", redirectResult.PageName);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithReport()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);

            var report = new IncidentReport
            {
                ReportID = 1,
                Title = "Edit Test",
                Location = "Pretoria",
                Description = "To be edited",
                Severity = IncidentSeverity.Minor,
                Status = IncidentStatus.Reported,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = System.DateTime.UtcNow
            };
            context.IncidentReports.Add(report);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // FIX: Add HttpContext
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IncidentReport>(viewResult.Model);
            Assert.Equal(1, model.ReportID);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);
            var report = new IncidentReport
            {
                ReportID = 1,
                Title = "Test",
                Location = "Test",
                Description = "Test",
                Severity = IncidentSeverity.Minor,
                Status = IncidentStatus.Reported,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = System.DateTime.UtcNow
            };

            // Act
            var result = await controller.Edit(2, report);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_UpdatesReport_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);

            var originalTimestamp = System.DateTime.UtcNow;
            var report = new IncidentReport
            {
                ReportID = 1,
                Title = "Original",
                Location = "Original Location",
                Description = "Original Description",
                Severity = IncidentSeverity.Minor,
                Status = IncidentStatus.Reported,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = originalTimestamp
            };
            context.IncidentReports.Add(report);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

            report.Title = "Updated Title";
            report.Description = "Updated Description";
            report.Status = IncidentStatus.Investigating;

            // Act
            var result = await controller.Edit(1, report);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedReport = await context.IncidentReports.FindAsync(1);
            Assert.Equal("Updated Title", updatedReport.Title);
            Assert.Equal("Updated Description", updatedReport.Description);
            Assert.Equal(IncidentStatus.Investigating, updatedReport.Status);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // Act
            var result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesReport_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "user1",
                Email = "test@example.com",
                UserName = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Public"
            };
            context.Users.Add(user);

            var report = new IncidentReport
            {
                ReportID = 1,
                Title = "To Delete",
                Location = "Delete Location",
                Description = "Will be deleted",
                Severity = IncidentSeverity.Minor,
                Status = IncidentStatus.Reported,
                ReportedByUserId = user.Id,
                ReportedByUser = user,
                Timestamp = System.DateTime.UtcNow
            };
            context.IncidentReports.Add(report);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = new IncidentReportsController(context, mockUserManager.Object);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var deletedReport = await context.IncidentReports.FindAsync(1);
            Assert.Null(deletedReport);
        }
    }
}