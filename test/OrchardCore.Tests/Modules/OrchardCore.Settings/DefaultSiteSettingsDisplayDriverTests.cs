using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Options;
using OrchardCore.Navigation;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Drivers;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Settings;

public class DefaultSiteSettingsDisplayDriverTests
{
    [Fact]
    public async Task UpdateAsync_ResourceSettingsChanged_RequestsResourceOptionsUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, model => model.UseCdn = true);

        Assert.Equal([typeof(ResourceOptions), typeof(ResourceManagementOptions)], notifier.Requests);
    }

    [Fact]
    public async Task UpdateAsync_PagerSettingsChanged_RequestsPagerOptionsUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, model => model.PageSize = 20);

        Assert.Equal([typeof(PagerOptions)], notifier.Requests);
    }

    [Fact]
    public async Task UpdateAsync_CacheModeChanged_RequestsCacheOptionsUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, model => model.CacheMode = CacheMode.Disabled);

        Assert.Equal([typeof(CacheOptions)], notifier.Requests);
    }

    [Fact]
    public async Task UpdateAsync_NothingChanged_RequestsNoUpdate()
    {
        var notifier = new RecordingOptionsUpdateNotifier();

        await UpdateAsync(notifier, model => model.SiteName = "Renamed");

        Assert.Empty(notifier.Requests);
    }

    private static async Task UpdateAsync(IOptionsUpdateNotifier notifier, Action<SiteSettingsViewModel> change)
    {
        var site = new SiteSettings
        {
            SiteName = "Site",
            PageSize = 10,
            PageSizeOptions = [10, 25],
            UseCdn = false,
            CacheMode = CacheMode.Enabled,
        };

        var model = new SiteSettingsViewModel
        {
            SiteName = site.SiteName,
            PageTitleFormat = site.PageTitleFormat,
            BaseUrl = site.BaseUrl,
            TimeZone = site.TimeZoneId,
            PageSize = site.PageSize,
            AllowPageSizeSelection = site.AllowPageSizeSelection,
            PageSizeOptions = string.Join(", ", site.PageSizeOptions),
            UseCdn = site.UseCdn,
            CdnBaseUrl = site.CdnBaseUrl,
            ResourceDebugMode = site.ResourceDebugMode,
            AppendVersion = site.AppendVersion,
            CacheMode = site.CacheMode,
        };

        change(model);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var driver = new DefaultSiteSettingsDisplayDriver(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            authorizationService.Object,
            notifier,
            Mock.Of<IStringLocalizer<DefaultSiteSettingsDisplayDriver>>());

        var context = new UpdateEditorContext(new Shape(), DefaultSiteSettingsDisplayDriver.GroupId, false, string.Empty, null, null, new ModelUpdater(model));

        await driver.UpdateAsync(site, context);
    }

    private sealed class RecordingOptionsUpdateNotifier : IOptionsUpdateNotifier
    {
        public List<Type> Requests { get; } = [];

        public IOptionsUpdateNotifier RequestUpdate<TOptions>(string name = "") where TOptions : class
        {
            Requests.Add(typeof(TOptions));

            return this;
        }

        public IOptionsUpdateNotifier RequestUpdateAll() => throw new NotSupportedException();
    }

    private sealed class ModelUpdater : IUpdateModel
    {
        private readonly SiteSettingsViewModel _model;

        public ModelUpdater(SiteSettingsViewModel model)
        {
            _model = model;
        }

        public ModelStateDictionary ModelState { get; } = new();

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
            => BindAsync(model);

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
            => BindAsync(model);

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
            => BindAsync(model);

        public bool TryValidateModel(object model) => true;

        public bool TryValidateModel(object model, string prefix) => true;

        private Task<bool> BindAsync<TModel>(TModel model)
        {
            if (model is SiteSettingsViewModel viewModel)
            {
                foreach (var property in typeof(SiteSettingsViewModel).GetProperties().Where(property => property.CanWrite))
                {
                    property.SetValue(viewModel, property.GetValue(_model));
                }
            }

            return Task.FromResult(true);
        }
    }
}
