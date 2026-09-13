using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataLocalization.Endpoints;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.Localization.Endpoints;
using OrchardCore.Localization.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Settings;
using static OrchardCore.Localization.Endpoints.LocalizationManagementEndpoints;
using static OrchardCore.DataLocalization.Endpoints.TranslationManagementEndpoints;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class LocalizationManagementTests
{
    [Theory]
    [InlineData("en", new[] { "en", "unknown-culture" })]
    [InlineData("fr", new[] { "en" })]
    [InlineData("en", new string[0])]
    public async Task Settings_InvalidCultures_DoesNotPersist(string defaultCulture, string[] supported)
    {
        var fixture = new Fixture();
        var result = await UpdateAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object,
            fixture.Site.Object, fixture.Release.Object, new CultureSettings { DefaultCulture = defaultCulture, SupportedCultures = supported });
        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task Settings_EquivalentCultureSet_DoesNotReleaseTenant()
    {
        var fixture = new Fixture();
        var result = await UpdateAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object,
            fixture.Site.Object, fixture.Release.Object, new CultureSettings { DefaultCulture = "EN", SupportedCultures = ["FR", "en", "fr"] });
        Assert.Equal(200, ((IStatusCodeHttpResult)result).StatusCode);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task Settings_Changed_DefaultIsSavedAndTenantReleased()
    {
        var fixture = new Fixture();
        var site = new SiteSettings();
        fixture.Site.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var result = await UpdateAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object,
            fixture.Site.Object, fixture.Release.Object, new CultureSettings { DefaultCulture = "FR", SupportedCultures = ["fr", "en"], FallBackToParentCulture = true });
        Assert.Equal(200, ((IStatusCodeHttpResult)result).StatusCode);
        Assert.Equal("fr", site.As<LocalizationSettings>().DefaultCulture);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(site), Times.Once);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Once);
    }

    [Fact]
    public async Task Cultures_Available_IncludesUnsupportedNamesAndPages()
    {
        var fixture = new Fixture();
        var result = await AvailableCulturesAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, new LocalizationListRequest { Take = 1 });
        var cultures = Assert.IsType<CultureListResponse>(((IValueHttpResult)result).Value);
        Assert.Equal(3, cultures.TotalCount);
        var culture = Assert.Single(cultures.Items);
        Assert.Equal("de", culture.Name);
        Assert.False(culture.IsSupported);
        Assert.False(culture.IsDefault);
    }

    [Theory]
    [InlineData(false, "DE", new[] { "de", "en", "fr" })]
    [InlineData(true, "FR", new[] { "en" })]
    public async Task Cultures_ChangeOne_PreservesDefaultAndFallback(bool remove, string culture, string[] expected)
    {
        var fixture = new Fixture();
        fixture.Localization.SetupGet(service => service.FallBackToParentCultures).Returns(true);
        var site = new SiteSettings();
        fixture.Site.Setup(service => service.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var result = remove
            ? await RemoveCultureAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Site.Object, fixture.Release.Object, culture)
            : await AddCultureAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Site.Object, fixture.Release.Object, culture);
        var settings = Assert.IsType<CultureSettings>(((IValueHttpResult)result).Value);
        Assert.Equal(expected, settings.SupportedCultures);
        Assert.Equal("en", settings.DefaultCulture);
        Assert.True(settings.FallBackToParentCulture);
        Assert.Equal(expected, site.As<LocalizationSettings>().SupportedCultures);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Once);
    }

    [Theory]
    [InlineData(false, "FR", 200)]
    [InlineData(true, "de", 200)]
    [InlineData(true, "EN", 400)]
    [InlineData(true, "unknown-culture", 400)]
    [InlineData(false, "unknown-culture", 400)]
    public async Task Cultures_RetryOrInvalidChange_DoesNotPersist(bool remove, string culture, int status)
    {
        var fixture = new Fixture();
        var result = remove
            ? await RemoveCultureAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Site.Object, fixture.Release.Object, culture)
            : await AddCultureAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Site.Object, fixture.Release.Object, culture);
        Assert.Equal(status, ((IStatusCodeHttpResult)result).StatusCode);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task Cultures_AndGroups_RequireManageCultures()
    {
        var fixture = new Fixture("AccessRemoteManagement");
        Assert.Equal(403, ((IStatusCodeHttpResult)await AvailableCulturesAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, new LocalizationListRequest())).StatusCode);
        Assert.Equal(403, ((IStatusCodeHttpResult)await AddCultureAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Site.Object, fixture.Release.Object, "de")).StatusCode);
        Assert.Equal(403, ((IStatusCodeHttpResult)await RemoveCultureAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Site.Object, fixture.Release.Object, "fr")).StatusCode);
        Assert.Equal(403, ((IStatusCodeHttpResult)await StringGroupsAsync(fixture.Context, fixture.Authorization.Object, [], new LocalizationListRequest())).StatusCode);
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task StringGroups_MergesAdvertisedNames_WithPagingAndLegacyProviderCompatibility()
    {
        var fixture = new Fixture();
        var first = new Mock<IJSLocalizer>();
        first.Setup(service => service.GetLocalizationGroups()).Returns(["z-group", "a-group", "", new string('a', 201)]);
        var second = new Mock<IJSLocalizer>();
        second.Setup(service => service.GetLocalizationGroups()).Returns(["a-group", "A-group"]);
        IJSLocalizer legacy = new LegacyJSLocalizer();
        Assert.Empty(legacy.GetLocalizationGroups());
        Assert.NotEmpty(legacy.GetLocalizations("legacy"));
        var result = await StringGroupsAsync(fixture.Context, fixture.Authorization.Object, [first.Object, second.Object, legacy], new LocalizationListRequest { Skip = 1, Take = 1 });
        var groups = Assert.IsType<UiStringGroupsResponse>(((IValueHttpResult)result).Value);
        Assert.Equal(3, groups.TotalCount);
        Assert.Equal("a-group", Assert.Single(groups.Items).Name);
        first.Verify(service => service.GetLocalizations(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 201)]
    public async Task StringGroups_InvalidPaging_IsRejected(int skip, int take)
    {
        var fixture = new Fixture();
        var result = await StringGroupsAsync(fixture.Context, fixture.Authorization.Object, [], new LocalizationListRequest { Skip = skip, Take = take });
        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
    }

    private sealed class LegacyJSLocalizer : IJSLocalizer
    {
        public IDictionary<string, string> GetLocalizations(string group) => new Dictionary<string, string> { ["Hello"] = "Hello" };
    }

    [Fact]
    public async Task Strings_ExplicitCulture_UsesAndRestoresRequestCulture()
    {
        var fixture = new Fixture();
        var original = CultureInfo.CurrentUICulture;
        var localizer = new Mock<IJSLocalizer>();
        localizer.Setup(service => service.GetLocalizations("example")).Returns(() => new Dictionary<string, string> { ["Hello"] = CultureInfo.CurrentUICulture.Name });
        var result = await StringsAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, [localizer.Object], "example", new UiStringsRequest { Culture = "FR" });
        var value = Assert.IsType<UiStringsResponse>(((IValueHttpResult)result).Value);
        Assert.Equal("fr", Assert.Single(value.Items).Value);
        Assert.Same(original, CultureInfo.CurrentUICulture);
    }

    [Fact]
    public async Task Translations_SetAndDelete_PreserveOtherEntriesAndCultures()
    {
        var fixture = new Fixture();
        fixture.Document.Translations["en"] = [new Translation { Context = "Content Types", Key = "Page", Value = "English page" }];
        fixture.Document.Translations["fr"] = [new Translation { Context = "Other", Key = "Unrelated", Value = "Conserver" }];
        var request = new TranslationRequest { Culture = "FR", Context = "Content Types", Key = "Page", Value = "Page française" };
        var first = await fixture.SetAsync(request);
        Assert.True(Assert.IsType<TranslationResult>(((IValueHttpResult)first).Value).Changed);
        var retry = await fixture.SetAsync(request);
        Assert.False(Assert.IsType<TranslationResult>(((IValueHttpResult)retry).Value).Changed);
        Assert.Equal(2, fixture.Document.Translations["fr"].Count());
        var deletion = new TranslationKeyRequest { Culture = "fr", Context = request.Context, Key = request.Key };
        var deleted = await TranslationManagementEndpoints.DeleteAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Manager.Object, deletion);
        Assert.True(Assert.IsType<TranslationResult>(((IValueHttpResult)deleted).Value).Changed);
        var repeated = await TranslationManagementEndpoints.DeleteAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Manager.Object, deletion);
        Assert.False(Assert.IsType<TranslationResult>(((IValueHttpResult)repeated).Value).Changed);
        Assert.Equal("Conserver", Assert.Single(fixture.Document.Translations["fr"]).Value);
        Assert.Equal("English page", Assert.Single(fixture.Document.Translations["en"]).Value);
        fixture.Manager.Verify(service => service.UpdateTranslationAsync("fr", It.IsAny<IEnumerable<Translation>>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData("fr", "Content Types", "Page", "", 400)]
    [InlineData("de", "Content Types", "Page", "Test", 400)]
    [InlineData("fr", "Content Types", "page", "Test", 404)]
    [InlineData("fr", "Unknown", "Page", "Test", 404)]
    public async Task Translations_InvalidInput_DoesNotPersist(string culture, string context, string key, string value, int status)
    {
        var fixture = new Fixture();
        var result = await fixture.SetAsync(new TranslationRequest { Culture = culture, Context = context, Key = key, Value = value });
        Assert.Equal(status, ((IStatusCodeHttpResult)result).StatusCode);
        fixture.Manager.Verify(service => service.UpdateTranslationAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Translation>>()), Times.Never);
    }

    [Theory]
    [InlineData("fr", 200)]
    [InlineData("en", 403)]
    public async Task Translations_CulturePermission_IsEnforced(string culture, int status)
    {
        var fixture = new Fixture("ManageTranslations_fr");
        var result = await fixture.SetAsync(new TranslationRequest { Culture = culture, Context = "Content Types", Key = "Page", Value = "Test" });
        Assert.Equal(status, ((IStatusCodeHttpResult)result).StatusCode);
    }

    [Fact]
    public async Task Reads_MissingPermissions_AreForbidden()
    {
        var fixture = new Fixture("AccessRemoteManagement");
        Assert.Equal(403, ((IStatusCodeHttpResult)await GetAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object)).StatusCode);
        Assert.Equal(403, ((IStatusCodeHttpResult)await LocalizationManagementEndpoints.ListAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, new CultureListRequest())).StatusCode);
        Assert.Equal(403, ((IStatusCodeHttpResult)await TranslationManagementEndpoints.ListAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object, fixture.Manager.Object, [fixture.Provider.Object], new TranslationListRequest { Culture = "fr" })).StatusCode);
    }

    [Fact]
    public async Task Translations_CompoundKeysAndPaging_KeepDistinctPairsAndStoredValues()
    {
        var fixture = new Fixture();
        fixture.Provider.Setup(service => service.GetDescriptorsAsync()).ReturnsAsync([
            new DataLocalizedString("A|B", "C", "First"),
            new DataLocalizedString("A", "B|C", "Second"),
        ]);
        fixture.Document.Translations["fr"] = [new Translation { Context = "A|B", Key = "C", Value = "Premier" }];
        var result = await TranslationManagementEndpoints.ListAsync(fixture.Context, fixture.Authorization.Object,
            fixture.Localization.Object, fixture.Manager.Object, [fixture.Provider.Object],
            new TranslationListRequest { Culture = "fr", Skip = 1, Take = 1 });
        var value = Assert.IsType<TranslationListResponse>(((IValueHttpResult)result).Value);
        Assert.Equal(2, value.TotalCount);
        var item = Assert.Single(value.Items);
        Assert.Equal("A|B", item.Context);
        Assert.Equal("Premier", item.Value);
        Assert.True(item.IsTranslated);
    }

    [Fact]
    public async Task Writes_MissingPermissions_DoNotPersistOrRelease()
    {
        var fixture = new Fixture("AccessRemoteManagement");
        var settings = await UpdateAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object,
            fixture.Site.Object, fixture.Release.Object, new CultureSettings { DefaultCulture = "fr", SupportedCultures = ["en", "fr"] });
        var translation = await fixture.SetAsync(new TranslationRequest { Culture = "fr", Context = "Content Types", Key = "Page", Value = "Page" });
        var deletion = await TranslationManagementEndpoints.DeleteAsync(fixture.Context, fixture.Authorization.Object, fixture.Localization.Object,
            fixture.Manager.Object, new TranslationKeyRequest { Culture = "fr", Context = "Content Types", Key = "Page" });
        Assert.All(new[] { settings, translation, deletion }, result => Assert.Equal(403, ((IStatusCodeHttpResult)result).StatusCode));
        fixture.Site.Verify(service => service.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        fixture.Release.Verify(service => service.RequestRelease(), Times.Never);
        fixture.Manager.Verify(service => service.UpdateTranslationAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Translation>>()), Times.Never);
    }

    [Fact]
    public async Task Routes_AllEleven_RequireAuthorizationButOnlyCultureSettingsExposeCliMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        await using var app = builder.Build();
        app.AddLocalizationManagementEndpoints();
        app.AddTranslationManagementEndpoints();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToArray();
        Assert.Equal(11, endpoints.Length);
        var commands = endpoints.Select(endpoint => endpoint.Metadata.GetMetadata<CliOperationMetadata>()).OfType<CliOperationMetadata>().ToArray();
        Assert.Equal(6, commands.Length);
        Assert.All(commands, command => Assert.Contains(command.CommandGroup[^1], new[] { "cultures", "settings" }));
        Assert.True(Assert.Single(commands, command => command.Verb == "remove").RequiresConfirmation);
        Assert.False(Assert.Single(commands, command => command.Verb == "add").RequiresConfirmation);
        Assert.All(endpoints, endpoint =>
        {
            Assert.Contains(endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(), data => data.AuthenticationSchemes == OrchardCoreConstants.AuthenticationSchemes.Api);
            Assert.Contains(endpoint.Metadata.GetOrderedMetadata<AuthorizationPolicy>().SelectMany(policy => policy.Requirements), requirement => requirement is PermissionRequirement permission && permission.Permission.Name == "AccessRemoteManagement");
            var route = Assert.IsType<RouteEndpoint>(endpoint).RoutePattern.RawText;
            if (route.Contains("/strings", StringComparison.Ordinal) || route.Contains("/translations", StringComparison.Ordinal))
            {
                Assert.Null(endpoint.Metadata.GetMetadata<CliOperationMetadata>());
            }
            else
            {
                Assert.NotNull(endpoint.Metadata.GetMetadata<CliOperationMetadata>());
            }
        });
    }

    private sealed class Fixture
    {
        public DefaultHttpContext Context { get; } = new();
        public Mock<IAuthorizationService> Authorization { get; } = new();
        public Mock<ILocalizationService> Localization { get; } = new();
        public Mock<ISiteService> Site { get; } = new();
        public Mock<IShellReleaseManager> Release { get; } = new();
        public Mock<ITranslationsManager> Manager { get; } = new();
        public Mock<ILocalizationDataProvider> Provider { get; } = new();
        public TranslationsDocument Document { get; } = new();

        public Fixture(string permission = "*")
        {
            Context.RequestServices = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
            Authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, _, requirements) => Task.FromResult(permission == "*" || requirements.OfType<PermissionRequirement>().All(item => item.Permission.Name == permission) ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
            Localization.Setup(service => service.GetDefaultCultureAsync()).ReturnsAsync("en");
            Localization.Setup(service => service.GetSupportedCulturesAsync()).ReturnsAsync(["en", "fr"]);
            Localization.Setup(service => service.GetAllCulturesAndAliases()).Returns([CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("de")]);
            Manager.Setup(service => service.LoadTranslationsDocumentAsync()).ReturnsAsync(Document);
            Manager.Setup(service => service.GetTranslationsDocumentAsync()).ReturnsAsync(Document);
            Manager.Setup(service => service.UpdateTranslationAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Translation>>()))
                .Callback<string, IEnumerable<Translation>>((culture, translations) => Document.Translations[culture] = translations.ToArray()).Returns(Task.CompletedTask);
            Provider.Setup(service => service.GetDescriptorsAsync()).ReturnsAsync([new DataLocalizedString("Content Types", "Page", "Page")]);
        }

        public Task<IResult> SetAsync(TranslationRequest request) => TranslationManagementEndpoints.SetAsync(Context, Authorization.Object, Localization.Object, Manager.Object, [Provider.Object], request);
    }
}
