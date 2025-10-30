using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using GiftOfTheGivers.Web.Data;
using System.Threading.Tasks;
using GiftOfTheGivers.Web.Models.Enums;

namespace GiftOfTheGivers.Tests.Controllers
{
    public class ReliefProjectsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfProjects()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Project",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Flood Relief",
                Location = "Cape Town",
                Description = "Emergency flood relief",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<ReliefProject>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithProject()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-40),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Drought Relief",
                Location = "Northern Cape",
                Description = "Water supply project",
                Status = ProjectStatus.Planning,
                StartDate = System.DateTime.UtcNow.AddDays(30),
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReliefProject>(viewResult.Model);
            Assert.Equal(1, model.ProjectID);
            Assert.Equal("Drought Relief", model.Title);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

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
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "New",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);
            var project = new ReliefProject
            {
                Title = "Fire Relief",
                Location = "Western Cape",
                Description = "Wildfire relief operations",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id
            };

            // Act
            var result = await controller.Create(project);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Single(context.ReliefProjects);
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelIsInvalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Test",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);
            var project = new ReliefProject
            {
                Title = "Invalid Project",
                Location = "Test Location",
                Description = "Test",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id
            };
            controller.ModelState.AddModelError("Title", "Invalid title");

            // Act
            var result = await controller.Create(project);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(project, viewResult.Model);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithProject()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Edit",
                LastName = "Test",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Edit Project",
                Location = "Test Location",
                Description = "To be edited",
                Status = ProjectStatus.Planning,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReliefProject>(viewResult.Model);
            Assert.Equal(1, model.ProjectID);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Test",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);
            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Test",
                Location = "Test",
                Description = "Test",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id
            };

            // Act
            var result = await controller.Edit(2, project);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_UpdatesProject_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Test",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Original Title",
                Location = "Original Location",
                Description = "Original Description",
                Status = ProjectStatus.Planning,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);
            project.Title = "Updated Title";
            project.Description = "Updated Description";
            project.Status = ProjectStatus.Active;

            // Act
            var result = await controller.Edit(1, project);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedProject = await context.ReliefProjects.FindAsync(1);
            Assert.Equal("Updated Title", updatedProject.Title);
            Assert.Equal("Updated Description", updatedProject.Description);
            Assert.Equal(ProjectStatus.Active, updatedProject.Status);
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenModelIsInvalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Test",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Test Project",
                Location = "Test Location",
                Description = "Test Description",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);
            controller.ModelState.AddModelError("Title", "Invalid title");

            // Act
            var result = await controller.Edit(1, project);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(project, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WithProject()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Delete",
                LastName = "Test",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "To Delete",
                Location = "Delete Location",
                Description = "Will be deleted",
                Status = ProjectStatus.Completed,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReliefProject>(viewResult.Model);
            Assert.Equal(1, model.ProjectID);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesProject_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Delete",
                LastName = "Coordinator",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-30),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.Add(coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Delete Me",
                Location = "Delete Location",
                Description = "To be deleted",
                Status = ProjectStatus.Completed,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();

            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var deletedProject = await context.ReliefProjects.FindAsync(1);
            Assert.Null(deletedProject);
        }

        [Fact]
        public async Task DeleteConfirmed_HandlesNullProject_Gracefully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new ReliefProjectsController(context);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}