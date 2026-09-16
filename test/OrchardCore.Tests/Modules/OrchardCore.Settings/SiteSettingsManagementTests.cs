using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Settings;
using OrchardCore.Settings.Drivers;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Settings.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Settings;

public class SiteSettingsManagementTests
{
    [Fact]
    public void ToResponse_ProjectsOnlySafeCoreSettings()
    {
        var site = CreateSite();
        site.SiteSalt = "secret";
        site.SuperUser = "admin";
        site.HomeRoute["controller"] = "Home";
        site.Properties["Internal"] = "value";

        var response = SiteSettingsManagementService.ToResponse(site);
        var json = JsonSerializer.SerializeToElement(response);
        var names = json.EnumerateObject().Select(property => property.Name).Order().ToArray();

        Assert.Equal(
            new[]
            {
                "appendVersion",
                "baseUrl",
                "cacheMode",
                "calendar",
                "cdnBaseUrl",
                "maxPageSize",
                "maxPagedCount",
                "pageSize",
                "pageTitleFormat",
                "resourceDebugMode",
                "siteName",
                "timeZoneId",
                "useCdn",
            }.Order(),
            names);
        Assert.DoesNotContain("secret", json.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("superUser", json.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("homeRoute", json.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("properties", json.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("""{"siteSalt":"secret"}""")]
    [InlineData("""{"superUser":"admin"}""")]
    [InlineData("""{"homeRoute":{}}""")]
    [InlineData("""{"properties":{}}""")]
    [InlineData("""{"id":1}""")]
    [InlineData("""{"pageSize":null}""")]
    public void UpdateRequest_InternalOrUnknownProperty_IsRejected(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SiteSettingsUpdateRequest>(json));
    }

    [Fact]
    public async Task UpdateAsync_Unauthorized_DoesNotLoadOrMutateSite()
    {
        var site = CreateSite();
        var siteService = new Mock<ISiteService>();
        var service = CreateService(siteService, authorized: false);

        var result = await service.UpdateAsync(new ClaimsPrincipal(), new SiteSettingsUpdateRequest { PageSize = 0 });

        Assert.True(result.IsForbidden);
        Assert.Equal(10, site.PageSize);
        siteService.Verify(value => value.LoadSiteSettingsAsync(), Times.Never);
        siteService.Verify(value => value.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidValues_DoesNotMutateOrPersist()
    {
        var site = CreateSite();
        var siteService = new Mock<ISiteService>();
        siteService.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var service = CreateService(siteService, shellReleaseManager: shellReleaseManager);

        var result = await service.UpdateAsync(new ClaimsPrincipal(), new SiteSettingsUpdateRequest
        {
            BaseUrl = "not-a-url",
            PageSize = 0,
            MaxPageSize = -1,
            MaxPagedCount = -1,
            ResourceDebugMode = "1",
        });

        Assert.Contains("baseUrl", result.Errors);
        Assert.Contains("pageSize", result.Errors);
        Assert.Contains("maxPageSize", result.Errors);
        Assert.Contains("maxPagedCount", result.Errors);
        Assert.Contains("resourceDebugMode", result.Errors);
        Assert.Equal("https://example.test", site.BaseUrl);
        Assert.Equal(10, site.PageSize);
        Assert.Equal(100, site.MaxPageSize);
        Assert.Equal(500, site.MaxPagedCount);
        siteService.Verify(value => value.UpdateSiteSettingsAsync(It.IsAny<ISite>()), Times.Never);
        shellReleaseManager.Verify(value => value.RequestRelease(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OmittedValuesArePreservedAndAllowedNullIsApplied()
    {
        var site = CreateSite();
        var siteService = new Mock<ISiteService>();
        siteService.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var service = CreateService(siteService);
        var request = JsonSerializer.Deserialize<SiteSettingsUpdateRequest>("""{"baseUrl":null}""");

        var result = await service.UpdateAsync(new ClaimsPrincipal(), request);

        Assert.Empty(result.Errors);
        Assert.Null(site.BaseUrl);
        Assert.Equal("Test site", site.SiteName);
        Assert.Equal(10, site.PageSize);
        siteService.Verify(value => value.UpdateSiteSettingsAsync(site), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_IdenticalRetryDoesNotPersistOrReleaseAgain()
    {
        var site = CreateSite();
        var siteService = new Mock<ISiteService>();
        siteService.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        var shellReleaseManager = new Mock<IShellReleaseManager>();
        var service = CreateService(siteService, shellReleaseManager: shellReleaseManager);
        var request = new SiteSettingsUpdateRequest { SiteName = "Updated site" };

        var first = await service.UpdateAsync(new ClaimsPrincipal(), request);
        var retry = await service.UpdateAsync(new ClaimsPrincipal(), request);

        Assert.Empty(first.Errors);
        Assert.Empty(retry.Errors);
        Assert.Equal("Updated site", retry.Value.SiteName);
        siteService.Verify(value => value.UpdateSiteSettingsAsync(site), Times.Once);
        shellReleaseManager.Verify(value => value.RequestRelease(), Times.Once);
    }

    [Fact]
    public void BuildCoreSchema_DescribesOnlyTheStrictMergePayload()
    {
        var schema = SiteSettingsManagementService.BuildCoreSchema();
        var properties = schema["properties"].AsObject();

        Assert.Equal("https://json-schema.org/draft/2020-12/schema", schema["$schema"].GetValue<string>());
        Assert.False(schema["additionalProperties"].GetValue<bool>());
        Assert.Contains("every property is optional", schema["description"].GetValue<string>(), StringComparison.Ordinal);
        Assert.Equal(13, properties.Count);
        Assert.False(properties.ContainsKey("siteSalt"));
        Assert.False(properties.ContainsKey("superUser"));
        Assert.False(properties.ContainsKey("homeRoute"));
        Assert.False(properties.ContainsKey("properties"));
        Assert.Equal(1, properties["pageSize"]["minimum"].GetValue<int>());
        Assert.Equal(0, properties["maxPageSize"]["minimum"].GetValue<int>());
        Assert.Equal(
            ["FromConfiguration", "Enabled", "Disabled"],
            properties["resourceDebugMode"]["enum"].AsArray().Select(value => value.GetValue<string>()));
        Assert.Equal(
            ["FromConfiguration", "Enabled", "DebugEnabled", "Disabled"],
            properties["cacheMode"]["enum"].AsArray().Select(value => value.GetValue<string>()));
        Assert.Equal(3, properties["baseUrl"]["anyOf"].AsArray().Count);
    }

    [Fact]
    public async Task GetSchemaAsync_SeparatesCoreAndRegisteredDiscoveryOnlySections()
    {
        var contributedSchema = new System.Text.Json.Nodes.JsonObject
        {
            ["type"] = "object",
            ["additionalProperties"] = false,
        };
        ISiteSettingsManagementSchemaProvider[] providers =
        [
            new TestSchemaProvider(
                new SiteSettingsManagementSchemaSection
                {
                    Name = "zeta",
                    DisplayName = "Zeta",
                    Description = "An explicitly contributed schema.",
                    Schema = contributedSchema,
                }),
            new TestSchemaProvider(
                new SiteSettingsManagementSchemaSection
                {
                    Name = "alpha",
                    DisplayName = "Alpha",
                    Schema = new System.Text.Json.Nodes.JsonObject { ["type"] = "object" },
                }),
        ];
        var service = CreateService(new Mock<ISiteService>(), schemaProviders: providers);

        var result = await service.GetSchemaAsync(new ClaimsPrincipal());

        Assert.False(result.IsForbidden);
        Assert.NotNull(result.Value.Core["properties"]);
        Assert.Equal(["alpha", "zeta"], result.Value.Sections.Select(section => section.Name));
        var section = result.Value.Sections[1];
        Assert.Same(contributedSchema, section.Schema);
        Assert.False(section.Readable);
        Assert.False(section.Writable);
        var json = JsonSerializer.Serialize(result.Value);
        Assert.Contains("\"core\"", json, StringComparison.Ordinal);
        Assert.Contains("\"sections\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("siteSalt", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("superUser", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CapabilityProvider_AdvertisesStableSettingsAndSectionCapabilities()
    {
        var capabilities = (await new SiteSettingsRemoteManagementCapabilityProvider().GetCapabilitiesAsync()).ToArray();

        Assert.Equal(["settings", "settings-sections"], capabilities.Select(capability => capability.Id));
        Assert.Equal(["Settings", "Settings Sections"], capabilities.Select(capability => capability.DisplayName));
        Assert.All(capabilities, capability => Assert.Equal(
            $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
            capability.Version));
    }

    [Fact]
    public void Startup_RegistersManagementServiceAndCapabilityProvider()
    {
        var services = new ServiceCollection();

        new global::OrchardCore.Settings.Startup().ConfigureServices(services);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(SiteSettingsManagementService) &&
                descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IRemoteManagementCapabilityProvider) &&
                descriptor.ImplementationType == typeof(SiteSettingsRemoteManagementCapabilityProvider));
    }

    [Fact]
    public void Endpoints_ExposeAuthorizationOpenApiAndCliMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.AddSiteSettingsManagementEndpoints();
        var endpoints = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();

        AssertOperation(endpoints, "api/settings", "GET", "ApiGetSiteSettings", "show", CliInputMode.Options, [200, 401, 403]);
        AssertOperation(endpoints, "api/settings", "PUT", "ApiUpdateSiteSettings", "update", CliInputMode.Json, [200, 400, 401, 403]);
        AssertOperation(endpoints, "api/settings/schema", "GET", "ApiGetSiteSettingsSchema", "schema", CliInputMode.Options, [200, 401, 403]);

        var update = FindEndpoint(endpoints, "api/settings", "PUT");
        var accepts = update.Metadata.GetMetadata<IAcceptsMetadata>();
        Assert.NotNull(accepts);
        Assert.False(accepts.IsOptional);
        Assert.Contains("application/json", accepts.ContentTypes);
    }

    private static void AssertOperation(
        RouteEndpoint[] endpoints,
        string route,
        string method,
        string endpointName,
        string verb,
        CliInputMode inputMode,
        int[] expectedStatusCodes)
    {
        var endpoint = FindEndpoint(endpoints, route, method);

        Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary));
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description));
        Assert.Contains("Settings", endpoint.Metadata.GetRequiredMetadata<ITagsMetadata>().Tags);

        var cli = endpoint.Metadata.GetRequiredMetadata<CliOperationMetadata>();
        Assert.Equal(["settings"], cli.CommandGroup);
        Assert.Equal(verb, cli.Verb);
        Assert.Equal(inputMode, cli.InputMode);
        Assert.Equal("settings", cli.Capability);

        Assert.Contains(
            endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>(),
            data => data.AuthenticationSchemes == OrchardCoreConstants.AuthenticationSchemes.Api);
        var requirements = endpoint.Metadata.GetOrderedMetadata<AuthorizationPolicy>()
            .SelectMany(policy => policy.Requirements)
            .OfType<PermissionRequirement>()
            .Select(requirement => requirement.Permission)
            .ToArray();
        Assert.Contains(RemoteManagementPermissions.AccessRemoteManagement, requirements);
        Assert.Contains(SettingsPermissions.ManageSettings, requirements);

        var statusCodes = endpoint.Metadata.GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Select(metadata => metadata.StatusCode)
            .ToHashSet();
        Assert.All(expectedStatusCodes, statusCode => Assert.Contains(statusCode, statusCodes));
    }

    private static RouteEndpoint FindEndpoint(RouteEndpoint[] endpoints, string route, string method)
        => endpoints.Single(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText?.Trim('/'), route.Trim('/'), StringComparison.Ordinal) &&
            endpoint.Metadata.GetRequiredMetadata<IHttpMethodMetadata>().HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase));

    private static SiteSettingsManagementService CreateService(
        Mock<ISiteService> siteService,
        bool authorized = true,
        Mock<IShellReleaseManager> shellReleaseManager = null,
        IEnumerable<ISiteSettingsManagementSchemaProvider> schemaProviders = null)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(value => value.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        return new SiteSettingsManagementService(
            authorizationService.Object,
            siteService.Object,
            (shellReleaseManager ?? new Mock<IShellReleaseManager>()).Object,
            CreateLocalizer(),
            schemaProviders ?? []);
    }

    private static IStringLocalizer<DefaultSiteSettingsDisplayDriver> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<DefaultSiteSettingsDisplayDriver>>();
        localizer.Setup(value => value[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        return localizer.Object;
    }

    private static SiteSettings CreateSite()
        => new()
        {
            SiteName = "Test site",
            PageTitleFormat = "{% page_title Site.SiteName %}",
            BaseUrl = "https://example.test",
            TimeZoneId = "Etc/UTC",
            PageSize = 10,
            MaxPageSize = 100,
            MaxPagedCount = 500,
            Calendar = "Gregorian",
            ResourceDebugMode = ResourceDebugMode.FromConfiguration,
            UseCdn = false,
            CdnBaseUrl = "https://cdn.example.test",
            AppendVersion = true,
            CacheMode = CacheMode.FromConfiguration,
        };

    private sealed class TestSchemaProvider(params SiteSettingsManagementSchemaSection[] sections)
        : ISiteSettingsManagementSchemaProvider
    {
        public ValueTask<IEnumerable<SiteSettingsManagementSchemaSection>> GetSchemaSectionsAsync(ClaimsPrincipal user)
            => ValueTask.FromResult<IEnumerable<SiteSettingsManagementSchemaSection>>(sections);
    }
}
