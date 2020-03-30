using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email;
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
        [Fact]
        public async Task UsersShouldNotBeAbleToRegisterIfNotAllowed()
        {
            var mockUserManager = UsersMockHelper.MockUserManager<IUser>().Object;
            var settings = new RegistrationSettings { UsersCanRegister = UserRegistrationType.NoRegistration };
            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { RegistrationSettings = settings }))
                    )
            );
            var mockSmtpService = Mock.Of<ISmtpService>();

            var controller = new RegistrationController(
                mockUserManager,
                Mock.Of<IAuthorizationService>(),
                mockSiteService,
                Mock.Of<INotifier>(),
                Mock.Of<IEmailAddressValidator>(),
                Mock.Of<ILogger<RegistrationController>>(),
                Mock.Of<IHtmlLocalizer<RegistrationController>>(),
                Mock.Of<IStringLocalizer<RegistrationController>>());

            var result = await controller.Register();
            Assert.IsType<NotFoundResult>(result);

            // Post
            result = await controller.Register(new RegisterViewModel());
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UsersShouldBeAbleToRegisterIfAllowed()
        {
            var mockUserManager = UsersMockHelper.MockUserManager<IUser>().Object;
            var settings = new RegistrationSettings { UsersCanRegister = UserRegistrationType.AllowRegistration };
            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { RegistrationSettings = settings }))
                    )
            );
            var mockSmtpService = Mock.Of<ISmtpService>();

            var controller = new RegistrationController(
                mockUserManager,
                Mock.Of<IAuthorizationService>(),
                mockSiteService,
                Mock.Of<INotifier>(),
                Mock.Of<IEmailAddressValidator>(),
                Mock.Of<ILogger<RegistrationController>>(),
                Mock.Of<IHtmlLocalizer<RegistrationController>>(),
                Mock.Of<IStringLocalizer<RegistrationController>>());

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(ISmtpService)))
                .Returns(mockSmtpService);
            mockServiceProvider
                .Setup(x => x.GetService(typeof(UserManager<IUser>)))
                .Returns(mockUserManager);
            mockServiceProvider
                .Setup(x => x.GetService(typeof(ISiteService)))
                .Returns(mockSiteService);
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IEnumerable<IRegistrationFormEvents>)))
                .Returns(Enumerable.Empty<IRegistrationFormEvents>());
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IUserService)))
                .Returns(Mock.Of<IUserService>());
            mockServiceProvider
                .Setup(x => x.GetService(typeof(SignInManager<IUser>)))
                .Returns(MockSignInManager(mockUserManager).Object);
            mockServiceProvider
                .Setup(x => x.GetService(typeof(ITempDataDictionaryFactory)))
                .Returns(Mock.Of<ITempDataDictionaryFactory>());
            mockServiceProvider
                .Setup(x => x.GetService(typeof(IObjectModelValidator)))
                .Returns(Mock.Of<IObjectModelValidator>());

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.RequestServices)
                .Returns(mockServiceProvider.Object);

            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var result = await controller.Register();
            Assert.IsType<ViewResult>(result);

            // Post
            result = await controller.Register(new RegisterViewModel());
            Assert.IsType<ViewResult>(result);
        }

        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager = null) where TUser : class
        {
            var context = new Mock<HttpContext>();
            var manager = userManager ?? UsersMockHelper.MockUserManager<TUser>().Object;
            return new Mock<SignInManager<TUser>>(
                manager,
                new HttpContextAccessor { HttpContext = context.Object },
                Mock.Of<IUserClaimsPrincipalFactory<TUser>>(),
                null,
                null,
                null,
                null)
            { CallBase = true };
        }
    }
}
