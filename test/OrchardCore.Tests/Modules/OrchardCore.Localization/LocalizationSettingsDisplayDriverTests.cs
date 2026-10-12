using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Localization.Drivers;
using OrchardCore.Localization.Models;
using OrchardCore.Localization.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Modules.OrchardCore.Localization;

public class LocalizationSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ValidSettings_RequestsOptionsUpdateWithoutReleasingTenant()
    {
        // Arrange
        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var optionsUpdateNotifier = new Mock<IOptionsUpdateNotifier>();
        optionsUpdateNotifier
            .Setup(notifier => notifier.RequestUpdate<RequestLocalizationOptions>(It.IsAny<string>()))
            .Returns(optionsUpdateNotifier.Object);

        var driver = CreateDriver(shellReleaseManager.Object, optionsUpdateNotifier.Object);
        var settings = new LocalizationSettings();
        var updater = new StubUpdater("[\"fr-FR\",\"es-ES\"]", "es-ES", fallBackToParentCultures: true);

        // Act
        await driver.UpdateAsync(new SiteSettings(), settings, CreateUpdateContext(updater));

        // Assert
        Assert.Equal("es-ES", settings.DefaultCulture);
        Assert.Equal(["fr-FR", "es-ES"], settings.SupportedCultures);
        Assert.True(settings.FallBackToParentCulture);
        shellReleaseManager.Verify(manager => manager.RequestRelease(), Times.Never());
        optionsUpdateNotifier.Verify(notifier => notifier.RequestUpdate<RequestLocalizationOptions>(It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task UpdateAsync_NoSupportedCulture_DoesNotRequestOptionsUpdate()
    {
        // Arrange
        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var optionsUpdateNotifier = new Mock<IOptionsUpdateNotifier>();

        var driver = CreateDriver(shellReleaseManager.Object, optionsUpdateNotifier.Object);
        var updater = new StubUpdater("[]", "es-ES", fallBackToParentCultures: true);

        // Act
        await driver.UpdateAsync(new SiteSettings(), new LocalizationSettings(), CreateUpdateContext(updater));

        // Assert
        Assert.False(updater.ModelState.IsValid);
        shellReleaseManager.Verify(manager => manager.RequestRelease(), Times.Never());
        optionsUpdateNotifier.Verify(notifier => notifier.RequestUpdate<RequestLocalizationOptions>(It.IsAny<string>()), Times.Never());
    }

    private static UpdateEditorContext CreateUpdateContext(IUpdateModel updater)
        => new(new Shape(), LocalizationSettingsDisplayDriver.GroupId, false, string.Empty, null, null, updater);

    private static LocalizationSettingsDisplayDriver CreateDriver(IShellReleaseManager shellReleaseManager, IOptionsUpdateNotifier optionsUpdateNotifier)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.SetupProperty(accessor => accessor.HttpContext, new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity("Test")),
        });

        var stringLocalizer = new Mock<IStringLocalizer<LocalizationSettingsDisplayDriver>>();
        stringLocalizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        // The driver is activated from a container so that the test doesn't depend on its exact constructor signature.
        var services = new ServiceCollection()
            .AddSingleton(shellReleaseManager)
            .AddSingleton(optionsUpdateNotifier)
            .AddSingleton(Mock.Of<INotifier>())
            .AddSingleton(Mock.Of<ILocalizationService>())
            .AddSingleton(httpContextAccessor.Object)
            .AddSingleton(authorizationService.Object)
            .AddSingleton(Options.Create(new CultureOptions()))
            .AddSingleton(Mock.Of<IHtmlLocalizer<LocalizationSettingsDisplayDriver>>())
            .AddSingleton(stringLocalizer.Object)
            .BuildServiceProvider();

        return ActivatorUtilities.CreateInstance<LocalizationSettingsDisplayDriver>(services);
    }

    private sealed class StubUpdater : IUpdateModel
    {
        private readonly string _supportedCultures;
        private readonly string _defaultCulture;
        private readonly bool _fallBackToParentCultures;

        public StubUpdater(string supportedCultures, string defaultCulture, bool fallBackToParentCultures)
        {
            _supportedCultures = supportedCultures;
            _defaultCulture = defaultCulture;
            _fallBackToParentCultures = fallBackToParentCultures;
        }

        public ModelStateDictionary ModelState { get; } = new();

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
            => Bind(model);

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
            => Bind(model);

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
            => Bind(model);

        public bool TryValidateModel(object model) => true;

        public bool TryValidateModel(object model, string prefix) => true;

        private Task<bool> Bind<TModel>(TModel model) where TModel : class
        {
            if (model is LocalizationSettingsViewModel viewModel)
            {
                viewModel.SupportedCultures = _supportedCultures;
                viewModel.DefaultCulture = _defaultCulture;
                viewModel.FallBackToParentCultures = _fallBackToParentCultures;
            }

            return Task.FromResult(true);
        }
    }
}
