using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Email;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
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
            var mockUserManager = MockUserManager<IUser>().Object;
            var settings = new RegistrationSettings { UsersCanRegister = false };
            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { RegistrationSettings = settings }))
                    )
            );
            var mockSmtpService = Mock.Of<ISmtpService>();

            var controller = new RegistrationController(
                Mock.Of<IUserService>(), 
                mockUserManager,
                MockSignInManager(mockUserManager).Object,
                Mock.Of<IAuthorizationService>(),
                mockSiteService,
                Mock.Of<INotifier>(),
                mockSmtpService, 
                Mock.Of<IShapeFactory>(),
                Mock.Of<IHtmlDisplay>(),
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
            var mockUserManager = MockUserManager<IUser>().Object;
            var settings = new RegistrationSettings { UsersCanRegister = true };
            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { RegistrationSettings = settings }))
                    )
            );
            var mockSmtpService = Mock.Of<ISmtpService>();

            var controller = new RegistrationController(
                Mock.Of<IUserService>(),
                mockUserManager,
                MockSignInManager(mockUserManager).Object,
                Mock.Of<IAuthorizationService>(),
                mockSiteService,
                Mock.Of<INotifier>(),
                mockSmtpService,
                Mock.Of<IShapeFactory>(),
                Mock.Of<IHtmlDisplay>(),
                Mock.Of<ILogger<RegistrationController>>(),
                Mock.Of<IHtmlLocalizer<RegistrationController>>(),
                Mock.Of<IStringLocalizer<RegistrationController>>());

            var result = await controller.Register();
            Assert.IsType<ViewResult>(result);

            // Post
            result = await controller.Register(new RegisterViewModel());
            Assert.IsType<ViewResult>(result);
        }

        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager = null) where TUser : class
        {
            var context = new Mock<HttpContext>();
            var manager = userManager ?? MockUserManager<TUser>().Object;
            return new Mock<SignInManager<TUser>>(
                manager,
                new HttpContextAccessor { HttpContext = context.Object },
                Mock.Of<IUserClaimsPrincipalFactory<TUser>>(),
                null,
                null,
                null)
            { CallBase = true };
        }

        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            IList<IUserValidator<TUser>> UserValidators = new List<IUserValidator<TUser>>();
            IList<IPasswordValidator<TUser>> PasswordValidators = new List<IPasswordValidator<TUser>>();

            var store = new Mock<IUserStore<TUser>>();
            UserValidators.Add(new UserValidator<TUser>());
            PasswordValidators.Add(new PasswordValidator<TUser>());
            var mgr = new Mock<UserManager<TUser>>(store.Object,
                null,
                null,
                UserValidators,
                PasswordValidators,
                null,
                null,
                null,
                null);
            return mgr;
        }
    }
}
