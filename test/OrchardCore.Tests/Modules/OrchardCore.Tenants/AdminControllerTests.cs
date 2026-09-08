using OrchardCore.Data;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes.Services;
using OrchardCore.Tenants;
using OrchardCore.Tenants.Controllers;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Tenants;

public class AdminControllerTests
{
    [Fact]
    public async Task Index_FilteredAndSortedSettings_ArePagedAfterFilteringAndSorting()
    {
        var settings = new[]
        {
            CreateSettings("zeta", "Zeta description", running: true),
            CreateSettings("alpha", "Alpha description", running: false),
            CreateSettings("gamma", "Gamma description", running: true),
            CreateSettings("beta", "Beta description", running: true),
        };
        var controller = CreateController(settings);

        var result = await controller.Index(
            new TenantIndexOptions
            {
                Status = TenantsState.Running,
                OrderBy = TenantsOrder.Name,
            },
            new PagerParameters
            {
                Page = 2,
                PageSize = 2,
            });

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminIndexViewModel>(viewResult.Model);
        var entry = Assert.Single(model.ShellSettingsEntries);
        Assert.Equal("zeta", entry.Name);
        Assert.Equal("Zeta description", entry.Description);

        var pager = Assert.IsAssignableFrom<IShape>((object)model.Pager);
        Assert.Equal(3, pager.Properties["TotalItemCount"]);
    }

    private static ShellSettings CreateSettings(string name, string description, bool running)
    {
        var settings = new ShellSettings
        {
            Name = name,
            ["Description"] = description,
        };

        return running ? settings.AsRunning() : settings.AsDisabled();
    }

    private static AdminController CreateController(IEnumerable<ShellSettings> settings)
    {
        var shellHost = new Mock<IShellHost>();
        shellHost
            .Setup(x => x.GetAllSettings())
            .Returns(settings);

        return new AdminController(
            shellHost.Object,
            Mock.Of<IShellSettingsManager>(),
            Mock.Of<IShellRemovalManager>(),
            CreateAuthorizationService().Object,
            new ShellSettings { Name = ShellSettings.DefaultShellName }.AsRunning(),
            Mock.Of<IFeatureProfilesService>(),
            [],
            new EphemeralDataProtectionProvider(),
            Mock.Of<IClock>(),
            Mock.Of<INotifier>(),
            Mock.Of<ITenantValidator>(),
            [],
            Options.Create(new PagerOptions { PageSize = 2 }),
            Options.Create(new TenantsOptions()),
            null,
            NullLogger<AdminController>.Instance,
            new TestShapeFactory(),
            Mock.Of<IStringLocalizer<AdminController>>(),
            Mock.Of<IHtmlLocalizer<AdminController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }

    private static Mock<IAuthorizationService> CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService;
    }

    private sealed class TestShapeFactory : IShapeFactory
    {
        public dynamic New => this;

        public async ValueTask<IShape> CreateAsync(
            string shapeType,
            Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating,
            Action<ShapeCreatedContext> created)
        {
            var shape = await shapeFactory();

            created?.Invoke(new ShapeCreatedContext
            {
                Shape = shape,
                ShapeFactory = this,
                ShapeType = shapeType,
            });

            return shape;
        }
    }
}
