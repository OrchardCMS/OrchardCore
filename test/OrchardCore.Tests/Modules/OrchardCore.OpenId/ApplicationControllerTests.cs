using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.Settings;
using Moq;
using OrchardCore.OpenId.Controllers;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Services;
using OrchardCore.OpenId.Abstractions.Managers;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Descriptor.Models;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.OpenId.ViewModels;
using OrchardCoreOpenId = OrchardCore.OpenId;
using System.Security.Claims;
using System.Threading;
using OrchardCore.Security.Permissions;


namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    public class ApplicationControllerTests
    {
        [Fact]
        public async Task UsersShouldNotBeAbleToCreateIfNotAllowed()
        {
            var mockUserManager = MockUserManager<IUser>().Object;
                        
            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<ISiteService>(),
                Mock.Of<IStringLocalizer<ApplicationController>>(),
                Mock.Of<IAuthorizationService>(),
                Mock.Of<IRoleProvider>(),
                Mock.Of<IOpenIdApplicationManager>(),
                mockUserManager,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>());

            var result = await controller.Create();
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UsersShouldBeAbleToCreateIfAllowed()
        {
            var mockUserManager = MockUserManager<IUser>().Object;                   

            var controller = new ApplicationController(
                Mock.Of<IShapeFactory>(),
                Mock.Of<ISiteService>(),
                Mock.Of<IStringLocalizer<ApplicationController>>(),
                AuthorizationServiceMockExtensionFactory().Object,
                Mock.Of<IRoleProvider>(),
                Mock.Of<IOpenIdApplicationManager>(),
                mockUserManager,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IHtmlLocalizer<ApplicationController>>(),
                Mock.Of<INotifier>(),
                Mock.Of<ShellDescriptor>());

            var result = await controller.Create();
            Assert.IsType<CreateOpenIdApplicationViewModel>(result);
        }

        //[Fact]
        //public async Task UsersShouldBeAbleToRegisterIfAllowed()
        //{
        //    var mockUserManager = MockUserManager<IUser>().Object;
        //    var settings = new RegistrationSettings { UsersCanRegister = true };
        //    var mockSiteService = Mock.Of<ISiteService>(ss =>
        //        ss.GetSiteSettingsAsync() == Task.FromResult(
        //            Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { RegistrationSettings = settings }))
        //            )
        //    );
        //    var mockSmtpService = Mock.Of<ISmtpService>();

        //    var controller = new RegistrationController(
        //        Mock.Of<IUserService>(),
        //        mockUserManager,
        //        MockSignInManager(mockUserManager).Object,
        //        Mock.Of<IAuthorizationService>(),
        //        mockSiteService,
        //        Mock.Of<INotifier>(),
        //        mockSmtpService,
        //        Mock.Of<IShapeFactory>(),
        //        Mock.Of<IHtmlDisplay>(),
        //        Mock.Of<ILogger<RegistrationController>>(),
        //        Mock.Of<IHtmlLocalizer<RegistrationController>>(),
        //        Mock.Of<IStringLocalizer<RegistrationController>>());

        //    var result = await controller.Register();
        //    Assert.IsType<ViewResult>(result);

        //    // Post
        //    result = await controller.Register(new RegisterViewModel());
        //    Assert.IsType<ViewResult>(result);
        //}

        //public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager = null) where TUser : class
        //{
        //    var context = new Mock<HttpContext>();
        //    var manager = userManager ?? MockUserManager<TUser>().Object;
        //    return new Mock<SignInManager<TUser>>(
        //        manager,
        //        new HttpContextAccessor { HttpContext = context.Object },
        //        Mock.Of<IUserClaimsPrincipalFactory<TUser>>(),
        //        null,
        //        null,
        //        null)
        //    { CallBase = true };
        //}

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

        public class TestUser : IUser
        {
            public string UserName { get; set; }
        }

        public Mock<IAuthorizationService> AuthorizationServiceMockExtensionFactory()
        {
            //var mockRepository = new Moq.MockRepository(Moq.MockBehavior.Strict);
            //var mockFactory = mockRepository.Create<IAuthorizationService>();
            //var ClaimsPrincipal = mockRepository.Create<ClaimsPrincipal>();
            //mockFactory.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Permission>())).ReturnsAsync(true);
            //return mockFactory;

            var mock = new Mock<IAuthorizationService>();
            mock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Permission>())).ReturnsAsync(true);
            mock.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Permission>(), It.IsAny<object>())).ReturnsAsync(true);
            return mock;

        }

    }
}
