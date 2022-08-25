using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using OpenIddict.Abstractions;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Controllers;
using OrchardCore.OpenId.ViewModels;
using Xunit;

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
            object[] mockData = new object[0];
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
                Mock.Of<ShellDescriptor>());

            controller.ControllerContext = CreateControllerContext();

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
                Mock.Of<ShellDescriptor>());

            controller.ControllerContext = CreateControllerContext();

            var model = new CreateOpenIdApplicationViewModel();
            model.Type = clientType;
            model.ClientSecret = clientSecret;
            model.AllowAuthorizationCodeFlow = allowAuthFlow;
            model.AllowPasswordFlow = allowPasswordFlow;
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
            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<IOptions<PagerOptions>>(),
                MockStringLocalizer().Object,
                MockAuthorizationServiceMock().Object,
                Mock.Of<IOpenIdApplicationManager>(),
                Mock.Of<IOpenIdScopeManager>(),
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>());

            controller.ControllerContext = CreateControllerContext();

            var model = new CreateOpenIdApplicationViewModel();
            model.Type = OpenIddictConstants.ClientTypes.Public;
            model.AllowAuthorizationCodeFlow = true;
            model.RedirectUris = uris;

            var validationContext = new ValidationContext(model);
            var localizerMock = new Mock<IStringLocalizer<CreateOpenIdApplicationViewModel>>();
            localizerMock.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
                .Returns((string name, object[] args) => new LocalizedString(name, string.Format(name, args)));
            validationContext.InitializeServiceProvider((t) => localizerMock.Object);

            foreach (var validation in model.Validate(validationContext))
            {
                controller.ModelState.AddModelError(validation.MemberNames.First(), validation.ErrorMessage);
            }

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

        public Mock<IAuthorizationService> MockAuthorizationServiceMock()
        {
            var securityMock = new Mock<IAuthorizationService>(MockBehavior.Strict);

            securityMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>())).Returns(Task.FromResult(AuthorizationResult.Success()));
            securityMock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Object>(), It.IsAny<string>())).Returns(Task.FromResult(AuthorizationResult.Success()));
            return securityMock;
        }

        public ControllerContext CreateControllerContext()
        {
            var mockContext = new Mock<HttpContext>(MockBehavior.Loose);
            mockContext.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

            return new ControllerContext()
            {
                HttpContext = mockContext.Object
            };
        }

        public Mock<IStringLocalizer<ApplicationController>> MockStringLocalizer()
        {
            var localizerMock = new Mock<IStringLocalizer<ApplicationController>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("TextToLocalize", "localizedText"));

            return localizerMock;
        }
    }
}
