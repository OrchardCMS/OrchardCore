using Microsoft.AspNetCore.Authentication;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class PasswordAuthenticationTimingTests
{
    [Fact]
    public async Task UserService_WhenUserDoesNotExist_InvokesTimingMitigation()
    {
        // Arrange
        var timingService = new Mock<IPasswordAuthenticationTimingService>();
        timingService.Setup(x => x.MitigateUnknownUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userManager = UsersMockHelper.MockUserManager<IUser>();
        userManager.Setup(x => x.FindByNameAsync("missing-user")).ReturnsAsync((IUser)null);
        userManager.Setup(x => x.FindByEmailAsync("missing-user")).ReturnsAsync((IUser)null);

        var service = new UserService(
            timingService.Object,
            MockSignInManager(userManager.Object).Object,
            userManager.Object,
            Options.Create(new IdentityOptions
            {
                User =
                {
                    RequireUniqueEmail = true,
                },
            }),
            [],
            [],
            Options.Create(new RegistrationOptions()),
            MockSiteService(new LoginSettings()).Object,
            [],
            Mock.Of<ILogger<UserService>>(),
            MockStringLocalizer<UserService>().Object);

        string error = null;

        // Act
        var user = await service.AuthenticateAsync("missing-user", "Password1!", (key, message) => error = message);

        // Assert
        Assert.Null(user);
        Assert.Equal("The specified username/password couple is invalid.", error);
        timingService.Verify(x => x.MitigateUnknownUserAsync("Password1!", It.IsAny<CancellationToken>()), Times.Once);
        timingService.Verify(x => x.DelayFailedAuthenticationAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AccountController_WhenUserDoesNotExist_InvokesTimingMitigationAndReturnsGenericError()
    {
        // Arrange
        var timingService = new Mock<IPasswordAuthenticationTimingService>();
        timingService.Setup(x => x.MitigateUnknownUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var userService = new Mock<IUserService>();
        userService.Setup(x => x.GetUserAsync("missing-user")).ReturnsAsync((IUser)null);

        var shape = Mock.Of<IShape>();
        var displayManager = new Mock<IDisplayManager<LoginForm>>();
        displayManager.Setup(x => x.UpdateEditorAsync(It.IsAny<LoginForm>(), It.IsAny<IUpdateModel>(), false, string.Empty, string.Empty))
            .Callback<LoginForm, IUpdateModel, bool, string, string>((model, _, _, _, _) =>
            {
                model.UserName = "missing-user";
                model.Password = "Password1!";
            })
            .ReturnsAsync(shape);

        var updateModelAccessor = new Mock<IUpdateModelAccessor>();
        updateModelAccessor.SetupGet(x => x.ModelUpdater).Returns(Mock.Of<IUpdateModel>());

        var controller = new AccountController(
            timingService.Object,
            userService.Object,
            MockSignInManager(UsersMockHelper.MockUserManager<IUser>().Object).Object,
            UsersMockHelper.MockUserManager<IUser>().Object,
            Mock.Of<ILogger<AccountController>>(),
            MockSiteService(new LoginSettings()).Object,
            MockHtmlLocalizer<AccountController>().Object,
            MockStringLocalizer<AccountController>().Object,
            [],
            Options.Create(new RegistrationOptions()),
            Mock.Of<INotifier>(),
            displayManager.Object,
            updateModelAccessor.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };

        // Act
        var result = await controller.LoginPOST();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(shape, viewResult.Model);
        Assert.Equal("Invalid login attempt.", Assert.Single(controller.ModelState[string.Empty].Errors).ErrorMessage);
        timingService.Verify(x => x.MitigateUnknownUserAsync("Password1!", It.IsAny<CancellationToken>()), Times.Once);
        timingService.Verify(x => x.DelayFailedAuthenticationAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Mock<SignInManager<IUser>> MockSignInManager(UserManager<IUser> userManager)
        => new(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<IUser>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<IUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<IUser>>());

    private static Mock<ISiteService> MockSiteService(LoginSettings loginSettings)
    {
        var site = new Mock<ISite>();
        site.Setup(x => x.GetOrCreate<LoginSettings>()).Returns(loginSettings);

        var siteService = new Mock<ISiteService>();
        siteService.Setup(x => x.GetSiteSettingsAsync()).ReturnsAsync(site.Object);

        return siteService;
    }

    private static Mock<IStringLocalizer<T>> MockStringLocalizer<T>()
    {
        var localizer = new Mock<IStringLocalizer<T>>();
        localizer.Setup(x => x[It.IsAny<string>()])
            .Returns<string>(name => new LocalizedString(name, name));
        localizer.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns<string, object[]>((name, arguments) => new LocalizedString(name, string.Format(CultureInfo.InvariantCulture, name, arguments)));

        return localizer;
    }

    private static Mock<IHtmlLocalizer<T>> MockHtmlLocalizer<T>()
    {
        var localizer = new Mock<IHtmlLocalizer<T>>();
        localizer.Setup(x => x[It.IsAny<string>()])
            .Returns<string>(name => new LocalizedHtmlString(name, name, false));
        localizer.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns<string, object[]>((name, arguments) => new LocalizedHtmlString(name, string.Format(CultureInfo.InvariantCulture, name, arguments), false));

        return localizer;
    }
}
