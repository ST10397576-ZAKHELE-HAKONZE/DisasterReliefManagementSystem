using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Xunit;

namespace GiftOfTheGivers.Tests.Unit.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            var logger = TestHelpers.CreateStubLogger<HomeController>();
            _controller = new HomeController(logger);

            // Mock HttpContext to prevent NullReferenceException
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var result = _controller.Privacy() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Error_ReturnsViewWithRequestId()
        {
            // Set Activity.Current for testing
            Activity.Current = new Activity("test").Start();

            var result = _controller.Error() as ViewResult;
            Assert.NotNull(result);
            var model = Assert.IsType<ErrorViewModel>(result.Model);
            Assert.NotNull(model.RequestId);

            Activity.Current?.Stop();
        }
    }
}