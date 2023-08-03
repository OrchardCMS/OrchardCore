using OpenIddict.Abstractions;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Controllers;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    public class ApplicationControllerTests
    {
        [Fact]
        public async Task UsersShouldNotBeAbleToCreateIfNotAllowed()
        {
            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<IOptions<PagerOptions>>(),
                Mock.Of<IStringLocalizer<ApplicationController>>(),
                Mock.Of<IAuthorizationService>(),
                Mock.Of<IOpenIdApplicationManager>(),
                Mock.Of<IOpenIdScopeManager>(),
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>());

            var result = await controller.Create();
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UsersShouldBeAbleToCreateApplicationIfAllowed()
        {
            var mockOpenIdScopeManager = new Mock<IOpenIdScopeManager>();
            var mockData = Array.Empty<object>();
            mockOpenIdScopeManager.Setup(m => m.ListAsync(null, null, default)).Returns(mockData.ToAsyncEnumerable());
            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<IOptions<PagerOptions>>(),
                Mock.Of<IStringLocalizer<ApplicationController>>(),
                MockAuthorizationServiceMock().Object,
                Mock.Of<IOpenIdApplicationManager>(),
                mockOpenIdScopeManager.Object,
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>())
            {
                ControllerContext = CreateControllerContext(),
            };

            var result = await controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Theory]
        [InlineData(OpenIddictConstants.ClientTypes.Public, "ClientSecret", true, false, false)]
        [InlineData(OpenIddictConstants.ClientTypes.Public, "", true, false, true)]
        [InlineData(OpenIddictConstants.ClientTypes.Confidential, "ClientSecret", true, false, true)]
        [InlineData(OpenIddictConstants.ClientTypes.Confidential, "", true, false, false)]
        public async Task ConfidentionalClientNeedsSecret(string clientType, string clientSecret, bool allowAuthFlow, bool allowPasswordFlow, bool expectValidModel)
        {
            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<IOptions<PagerOptions>>(),
                MockStringLocalizer().Object,
                MockAuthorizationServiceMock().Object,
                Mock.Of<IOpenIdApplicationManager>(),
                Mock.Of<IOpenIdScopeManager>(),
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>())
            {
                ControllerContext = CreateControllerContext(),
            };

            var model = new CreateOpenIdApplicationViewModel
            {
                Type = clientType,
                ClientSecret = clientSecret,
                AllowAuthorizationCodeFlow = allowAuthFlow,
                AllowPasswordFlow = allowPasswordFlow,
            };

            var result = await controller.Create(model);
            if (expectValidModel)
            {
                Assert.IsType<RedirectToActionResult>(result);
            }
            else
            {
                Assert.IsType<ViewResult>(result);
            }
            Assert.Equal(expectValidModel, controller.ModelState.IsValid);
        }

        [Theory]
        [InlineData("nonUrlString", false)]
        [InlineData("http://localhost http://localhost:8080 nonUrlString", false)]
        [InlineData("http://localhost http://localhost:8080", true)]
        public async Task RedirectUrisAreValid(string uris, bool expectValidModel)
        {
            // Arrange
            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<IOptions<PagerOptions>>(),
                MockStringLocalizer().Object,
                MockAuthorizationServiceMock().Object,
                Mock.Of<IOpenIdApplicationManager>(),
                Mock.Of<IOpenIdScopeManager>(),
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>())
            {
                ControllerContext = CreateControllerContext(),
            };

            var model = new CreateOpenIdApplicationViewModel
            {
                Type = OpenIddictConstants.ClientTypes.Public,
                AllowAuthorizationCodeFlow = true,
                ClientId = "123",
                DisplayName = "Name",
                RedirectUris = uris
            };

            ValidateControllerModel(controller, model);

            // Act
            var result = await controller.Create(model);

            // Assert
            Assert.Equal(expectValidModel, controller.ModelState.IsValid);

            if (expectValidModel)
            {
                Assert.IsType<RedirectToActionResult>(result);
            }
            else
            {
                Assert.IsType<ViewResult>(result);
            }
        }

#pragma warning disable CA1822 // Mark members as static
        public Mock<IAuthorizationService> MockAuthorizationServiceMock()
        {
            var securityMock = new Mock<IAuthorizationService>(MockBehavior.Strict);

            securityMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>())).Returns(Task.FromResult(AuthorizationResult.Success()));
            securityMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>())).Returns(Task.FromResult(AuthorizationResult.Success()));
            return securityMock;
        }

        public ControllerContext CreateControllerContext()
        {
            var mockContext = new Mock<HttpContext>(MockBehavior.Loose);
            mockContext.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

            return new ControllerContext()
            {
                HttpContext = mockContext.Object,
            };
        }

        public Mock<IStringLocalizer<ApplicationController>> MockStringLocalizer()
#pragma warning restore CA1822 // Mark members as static
        {
            var localizerMock = new Mock<IStringLocalizer<ApplicationController>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("TextToLocalize", "localizedText"));

            return localizerMock;
        }

        private static void ValidateControllerModel(Controller controller, object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);

            Validator.TryValidateObject(model, context, results, true);

            foreach (var result in results)
            {
                controller.ModelState.AddModelError(result.MemberNames.First(), result.ErrorMessage);
            }
        }
    }
}
