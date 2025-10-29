using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Models;
using System.Threading.Tasks;
using GiftOfTheGivers.Web.Models.Enums;

namespace GiftOfTheGivers.Tests.Controllers
{
    public class VolunteerAssignmentsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfAssignments()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "John",
                LastName = "Volunteer",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Jane",
                LastName = "Coordinator",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Flood Relief",
                Location = "Cape Town",
                Description = "Emergency relief",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.FieldSupport,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Pending,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<VolunteerAssignment>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Flood Relief", model[0].Project.Title);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenAssignmentDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithAssignment()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Alice",
                LastName = "Volunteer",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-28),
                Gender = "Female",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Bob",
                LastName = "Coordinator",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-40),
                Gender = "Male",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Drought Relief",
                Location = "Limpopo",
                Description = "Water distribution",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Logistics,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Active,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VolunteerAssignment>(viewResult.Model);
            Assert.Equal(1, model.AssignmentID);
            Assert.Equal(VolunteerRole.Logistics, model.Role);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

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
                Id = "volunteer1",
                Email = "newvolunteer@example.com",
                UserName = "newvolunteer@example.com",
                FirstName = "New",
                LastName = "Volunteer",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-22),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

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

            var controller = new VolunteerAssignmentsController(context);
            var assignment = new VolunteerAssignment
            {
                Role = VolunteerRole.Sorter,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Pending,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };

            // Act
            var result = await controller.Create(assignment);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Single(context.VolunteerAssignments);
        }

        [Fact]
        public async Task Create_Post_SetsAssignedDate_WhenNotProvided()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

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

            var controller = new VolunteerAssignmentsController(context);
            var assignment = new VolunteerAssignment
            {
                Role = VolunteerRole.AdminSupport,
                AssignedDate = default(System.DateTime), // Not set
                Status = AssignmentStatus.Pending,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };

            // Act
            var result = await controller.Create(assignment);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            var savedAssignment = await context.VolunteerAssignments.FirstOrDefaultAsync(a => a.UserId == user.Id);
            Assert.NotNull(savedAssignment);
            Assert.NotEqual(default(System.DateTime), savedAssignment.AssignedDate);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenAssignmentDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithAssignment()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Edit",
                LastName = "Test",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Edit Project",
                Location = "Edit Location",
                Description = "Edit Description",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Outreach,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Active,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VolunteerAssignment>(viewResult.Model);
            Assert.Equal(1, model.AssignmentID);
            Assert.Equal(VolunteerRole.Outreach, model.Role);
        }

        [Fact]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Test",
                LastName = "User",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

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

            var controller = new VolunteerAssignmentsController(context);
            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Logistics,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Active,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };

            // Act
            var result = await controller.Edit(2, assignment);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_UpdatesAssignment_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Update",
                LastName = "Test",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Original Project",
                Location = "Original Location",
                Description = "Original Description",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Sorter,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Pending,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);
            assignment.Role = VolunteerRole.FieldSupport;
            assignment.Status = AssignmentStatus.Active;

            // Act
            var result = await controller.Edit(1, assignment);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var updatedAssignment = await context.VolunteerAssignments.FindAsync(1);
            Assert.Equal(VolunteerRole.FieldSupport, updatedAssignment.Role);
            Assert.Equal(AssignmentStatus.Active, updatedAssignment.Status);
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenModelIsInvalid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Invalid",
                LastName = "Test",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

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

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.AdminSupport,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Pending,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);
            controller.ModelState.AddModelError("Role", "Invalid role");

            // Act
            var result = await controller.Edit(1, assignment);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(assignment, viewResult.Model);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenAssignmentDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WithAssignment()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Delete",
                LastName = "Test",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Delete Project",
                Location = "Delete Location",
                Description = "Delete Description",
                Status = ProjectStatus.Completed,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Logistics,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Completed,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VolunteerAssignment>(viewResult.Model);
            Assert.Equal(1, model.AssignmentID);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesAssignment_AndRedirectsToIndex()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Delete",
                LastName = "Me",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project = new ReliefProject
            {
                ProjectID = 1,
                Title = "Delete Project",
                Location = "Delete Location",
                Description = "To be deleted",
                Status = ProjectStatus.Completed,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.Add(project);

            var assignment = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Outreach,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Cancelled,
                UserId = user.Id,
                User = user,
                ProjectID = project.ProjectID,
                Project = project
            };
            context.VolunteerAssignments.Add(assignment);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var deletedAssignment = await context.VolunteerAssignments.FindAsync(1);
            Assert.Null(deletedAssignment);
        }

        [Fact]
        public async Task DeleteConfirmed_HandlesNullAssignment_Gracefully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Index_OrdersAssignmentsByProjectTitle()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var user = new ApplicationUser
            {
                Id = "volunteer1",
                Email = "volunteer@example.com",
                UserName = "volunteer@example.com",
                FirstName = "Test",
                LastName = "Volunteer",
                IDNumber = "1234567890123",
                DateOfBirth = System.DateTime.Now.AddYears(-25),
                Gender = "Male",
                UserType = "Volunteer"
            };
            var coordinator = new ApplicationUser
            {
                Id = "coord1",
                Email = "coordinator@example.com",
                UserName = "coordinator@example.com",
                FirstName = "Coordinator",
                LastName = "Name",
                IDNumber = "9876543210987",
                DateOfBirth = System.DateTime.Now.AddYears(-35),
                Gender = "Female",
                UserType = "Staff"
            };
            context.Users.AddRange(user, coordinator);

            var project1 = new ReliefProject
            {
                ProjectID = 1,
                Title = "Z Project",
                Location = "Location",
                Description = "Description",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            var project2 = new ReliefProject
            {
                ProjectID = 2,
                Title = "A Project",
                Location = "Location",
                Description = "Description",
                Status = ProjectStatus.Active,
                StartDate = System.DateTime.UtcNow,
                CoordinatorId = coordinator.Id,
                Coordinator = coordinator
            };
            context.ReliefProjects.AddRange(project1, project2);

            var assignment1 = new VolunteerAssignment
            {
                AssignmentID = 1,
                Role = VolunteerRole.Logistics,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Active,
                UserId = user.Id,
                User = user,
                ProjectID = project1.ProjectID,
                Project = project1
            };
            var assignment2 = new VolunteerAssignment
            {
                AssignmentID = 2,
                Role = VolunteerRole.Sorter,
                AssignedDate = System.DateTime.UtcNow,
                Status = AssignmentStatus.Active,
                UserId = user.Id,
                User = user,
                ProjectID = project2.ProjectID,
                Project = project2
            };
            context.VolunteerAssignments.AddRange(assignment1, assignment2);
            await context.SaveChangesAsync();

            var controller = new VolunteerAssignmentsController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<VolunteerAssignment>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("A Project", model[0].Project.Title);
            Assert.Equal("Z Project", model[1].Project.Title);
        }
    }
}