using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;
using Xunit;

namespace OrchardCore.Tests.OrchardCore.Users
{
    public class RegistrationControllerTests
    {
        private static readonly Mock<IServiceProvider> _mockServiceProvider = new Mock<IServiceProvider>();

        [Fact]
        public async Task UsersShouldNotBeAbleToRegisterIfNotAllowed()
        {
            // Arrange
            var controller = SetupRegistrationController(new RegistrationSettings
            {
                UsersCanRegister = UserRegistrationType.NoRegistration
            });

            // Act & Assert
            var result = await controller.Register();
            Assert.IsType<NotFoundResult>(result);

            result = await controller.Register(new RegisterViewModel());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UsersShouldBeAbleToRegisterIfAllowed()
        {
            // Arrange
            var controller = SetupRegistrationController();

            // Act & Assert
            var result = await controller.Register();
            Assert.IsType<ViewResult>(result);

            result = await controller.Register(new RegisterViewModel());
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task UsersShouldNotBeAbleToRegisterIfEmailIsDuplicate()
        {
            // Arrange

            var controller = SetupRegistrationController();

            // Act
            var result = await controller.Register(new RegisterViewModel { UserName = "SuperAdmin", Email = "admin@orchardcore.net" });
            result = await controller.Register(new RegisterViewModel { UserName = "Admin", Email = "admin@orchardcore.net" });

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ViewData.ModelState.IsValid);
            Assert.True(controller.ViewData.ModelState["Email"].Errors.Count == 1);
            Assert.Equal("A user with the same email already exists.", controller.ViewData.ModelState["Email"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task UsersCanRequireModeration()
        {
            // Arrange
            var controller = SetupRegistrationController(new RegistrationSettings
            {
                UsersCanRegister = UserRegistrationType.AllowRegistration,
                UsersAreModerated = true,
            });

            // Act
            var result = await controller.Register(new RegisterViewModel { UserName = "ModerateMe", Email = "requiresmoderation@orchardcore.net" });

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RegistrationPending", ((RedirectToActionResult)result).ActionName);
        }

        [Fact]
        public async Task UsersCanRequireEmailConfirmation()
        {
            // Arrange
            var controller = SetupRegistrationController(new RegistrationSettings
            {
                UsersCanRegister = UserRegistrationType.AllowRegistration,
                UsersMustValidateEmail = true
            });
            var shellContext = new ShellContext
            {
                Settings = new ShellSettings() { Name = ShellHelper.DefaultShellName, State = TenantState.Running },
                ServiceProvider = _mockServiceProvider.Object,
            };

            // Act & Assert
            await shellContext.CreateScope().UsingAsync(async _ =>
            {
                var result = await controller.Register(new RegisterViewModel { UserName = "ConfirmMe", Email = "requiresemailconfirmation@orchardcore.net" });

                Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("ConfirmEmailSent", ((RedirectToActionResult)result).ActionName);
            });
        }

        private static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager = null) where TUser : class, IUser
        {
            var context = new Mock<HttpContext>();
            var manager = userManager ?? UsersMockHelper.MockUserManager<TUser>().Object;

            var signInManager = new Mock<SignInManager<TUser>>(
                manager,
                new HttpContextAccessor { HttpContext = context.Object },
                Mock.Of<IUserClaimsPrincipalFactory<TUser>>(),
                null,
                null,
                null,
                null)
            { CallBase = true };

            signInManager.Setup(x => x.SignInAsync(It.IsAny<TUser>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return signInManager;
        }

        private static RegistrationController SetupRegistrationController()
            => SetupRegistrationController(new RegistrationSettings
            {
                UsersCanRegister = UserRegistrationType.AllowRegistration
            });

        private static RegistrationController SetupRegistrationController(RegistrationSettings registrationSettings)
        {
            var users = new List<IUser>();
            var mockUserManager = UsersMockHelper.MockUserManager<IUser>();
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .Returns<string>(e =>
                {
                    var user = users.SingleOrDefault(u => (u as User).Email == e);

                    return Task.FromResult(user);
                });
            mockUserManager
                .Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<IUser>()))
                .Returns(Task.FromResult("123456"));

            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { RegistrationSettings = registrationSettings }))
                    )
            );
            var mockSmtpService = Mock.Of<ISmtpService>(x => x.SendAsync(It.IsAny<MailMessage>()) == Task.FromResult(SmtpResult.Success));

            var mockStringLocalizer = new Mock<IStringLocalizer<RegistrationController>>();
            mockStringLocalizer.Setup(l => l[It.IsAny<string>()])
                .Returns<string>(s => new LocalizedString(s, s));

            var userService = new Mock<IUserService>();
            userService.Setup(u => u.CreateUserAsync(It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<Action<string, string>>()))
                .Callback<IUser, string, Action<string, string>>((u, p, e) => users.Add(u))
                .ReturnsAsync<IUser, string, Action<string, string>, IUserService, IUser>((u, p, e) => u);

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(uh => uh.Action(It.IsAny<UrlActionContext>()));

            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            var mockDisplayHelper = new Mock<IDisplayHelper>();
            mockDisplayHelper.Setup(x => x.ShapeExecuteAsync(It.IsAny<IShape>()))
                .ReturnsAsync(HtmlString.Empty);

            var mockSignInManager = MockSignInManager(mockUserManager.Object);

            var mockUserControllerService = new DefaultUserControllerService(
                mockSmtpService,
                Enumerable.Empty<IRegistrationFormEvents>(),
                userService.Object,
                mockSiteService,
                mockSignInManager.Object,
                mockUserManager.Object,
                NullLoggerFactory.Instance.CreateLogger<DefaultUserControllerService>());

            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            mockServiceScopeFactory.Setup(ssf => ssf.CreateScope()).Returns(() =>
            {
                var scope = new Mock<IServiceScope>();
                scope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            
                return scope.Object;
            });

            var controller = new RegistrationController(
                mockUserControllerService,
                mockUserManager.Object,
                Mock.Of<IAuthorizationService>(),
                mockSiteService,
                Mock.Of<INotifier>(),
                new EmailAddressValidator(),
                Mock.Of<ILogger<RegistrationController>>(),
                Mock.Of<IHtmlLocalizer<RegistrationController>>(),
                mockStringLocalizer.Object);

            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(mockServiceScopeFactory.Object);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IDistributedLock)))
                .Returns(new LocalLock(NullLoggerFactory.Instance.CreateLogger<LocalLock>()));
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<IModularTenantEvents>)))
                .Returns(Enumerable.Empty<IModularTenantEvents>());
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(ISmtpService)))
                .Returns(mockSmtpService);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(UserManager<IUser>)))
                .Returns(mockUserManager.Object);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(ISiteService)))
                .Returns(mockSiteService);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<IRegistrationFormEvents>)))
                .Returns(Enumerable.Empty<IRegistrationFormEvents>());
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUserService)))
                .Returns(userService.Object);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(SignInManager<IUser>)))
                .Returns(mockSignInManager.Object);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(Mock.Of<ITempDataDictionaryFactory>());
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IObjectModelValidator)))
                .Returns(Mock.Of<IObjectModelValidator>());
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IDisplayHelper)))
                .Returns(mockDisplayHelper.Object);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(IUrlHelperFactory)))
                .Returns(mockUrlHelperFactory.Object);
            _mockServiceProvider
                .Setup(x => x.GetService(typeof(HtmlEncoder)))
                .Returns(HtmlEncoder.Default);

            // var mockRequest = new Mock<HttpRequest>();
            // mockRequest.Setup(x => x.Scheme)
            //     .Returns("http");

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.RequestServices)
                .Returns(_mockServiceProvider.Object);
            mockHttpContext
                .Setup(x => x.Request)
                .Returns(Mock.Of<HttpRequest>(x => x.Scheme == "http"));

            urlHelperMock
                .Setup(uh => uh.ActionContext)
                .Returns(new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor()));

            controller.Url = urlHelperMock.Object;
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            return controller;
        }
    }
}
