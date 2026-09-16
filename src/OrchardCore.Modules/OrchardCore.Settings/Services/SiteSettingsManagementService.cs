using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.RemoteManagement;
using OrchardCore.Settings.Endpoints.Api;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.Settings.Services;

internal sealed class SiteSettingsManagementService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumerable<ISiteSettingsManagementSchemaProvider> _schemaProviders;

    public SiteSettingsManagementService(
        IAuthorizationService authorizationService,
        ISiteService siteService,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<DefaultSiteSettingsDisplayDriver> localizer,
        IEnumerable<ISiteSettingsManagementSchemaProvider> schemaProviders)
    {
        _authorizationService = authorizationService;
        _siteService = siteService;
        _shellReleaseManager = shellReleaseManager;
        _localizer = localizer;
        _schemaProviders = schemaProviders;
    }

    public async Task<SiteSettingsManagementResult<SiteSettingsResponse>> GetAsync(ClaimsPrincipal user)
    {
        if (!await _authorizationService.AuthorizeAsync(user, SettingsPermissions.ManageSettings))
        {
            return SiteSettingsManagementResult<SiteSettingsResponse>.Forbidden();
        }

        return SiteSettingsManagementResult<SiteSettingsResponse>.Success(ToResponse(await _siteService.GetSiteSettingsAsync()));
    }

    public async Task<SiteSettingsManagementResult<SiteSettingsResponse>> UpdateAsync(
        ClaimsPrincipal user,
        SiteSettingsUpdateRequest request)
    {
        if (!await _authorizationService.AuthorizeAsync(user, SettingsPermissions.ManageSettings))
        {
            return SiteSettingsManagementResult<SiteSettingsResponse>.Forbidden();
        }

        if (request is null)
        {
            return SiteSettingsManagementResult<SiteSettingsResponse>.Invalid(
                new Dictionary<string, string[]> { ["body"] = ["A site settings payload is required."] });
        }

        var site = await _siteService.LoadSiteSettingsAsync();
        var proposed = CreateProposedSettings(site, request);
        var errors = Validate(request, proposed);
        if (errors.Count > 0)
        {
            return SiteSettingsManagementResult<SiteSettingsResponse>.Invalid(errors);
        }

        if (Matches(site, proposed))
        {
            return SiteSettingsManagementResult<SiteSettingsResponse>.Success(ToResponse(site));
        }

        Apply(site, proposed);
        await _siteService.UpdateSiteSettingsAsync(site);
        _shellReleaseManager.RequestRelease();

        return SiteSettingsManagementResult<SiteSettingsResponse>.Success(ToResponse(site));
    }

    public async Task<SiteSettingsManagementResult<SiteSettingsManagementSchemaResponse>> GetSchemaAsync(ClaimsPrincipal user)
    {
        if (!await _authorizationService.AuthorizeAsync(user, SettingsPermissions.ManageSettings))
        {
            return SiteSettingsManagementResult<SiteSettingsManagementSchemaResponse>.Forbidden();
        }

        var sections = new List<SiteSettingsManagementSchemaSectionResponse>();
        foreach (var provider in _schemaProviders)
        {
            var contributedSections = await provider.GetSchemaSectionsAsync(user);
            if (contributedSections is null)
            {
                continue;
            }

            sections.AddRange(contributedSections
                .Where(section => section is not null && !string.IsNullOrWhiteSpace(section.Name) && section.Schema is not null)
                .Select(section => new SiteSettingsManagementSchemaSectionResponse
                {
                    Name = section.Name,
                    DisplayName = section.DisplayName,
                    Description = section.Description,
                    Schema = section.Schema,
                }));
        }

        var distinctSections = sections
            .OrderBy(section => section.Name, StringComparer.OrdinalIgnoreCase)
            .DistinctBy(section => section.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return SiteSettingsManagementResult<SiteSettingsManagementSchemaResponse>.Success(
            new SiteSettingsManagementSchemaResponse
            {
                Core = BuildCoreSchema(),
                Sections = distinctSections,
            });
    }

    internal static SiteSettingsResponse ToResponse(ISite site)
        => new()
        {
            SiteName = site.SiteName,
            PageTitleFormat = site.PageTitleFormat,
            BaseUrl = site.BaseUrl,
            TimeZoneId = site.TimeZoneId,
            PageSize = site.PageSize,
            MaxPageSize = site.MaxPageSize,
            MaxPagedCount = site.MaxPagedCount,
            Calendar = site.Calendar,
            ResourceDebugMode = site.ResourceDebugMode.ToString(),
            UseCdn = site.UseCdn,
            CdnBaseUrl = site.CdnBaseUrl,
            AppendVersion = site.AppendVersion,
            CacheMode = site.CacheMode.ToString(),
        };

    internal static JsonObject BuildCoreSchema()
        => new()
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["title"] = "Site settings update",
            ["description"] = "A JSON object containing safe core site settings. The request body is required, every property is optional, and omitted properties keep their current values.",
            ["type"] = "object",
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["siteName"] = NullableString("The site name.", "My Orchard Project Application"),
                ["pageTitleFormat"] = NullableString("The Liquid page title format.", "{% page_title Site.SiteName, position: \"after\", separator: \" - \" %}"),
                ["baseUrl"] = BaseUrl(),
                ["timeZoneId"] = NullableString("The site's time zone identifier, or null to use the configured default.", null),
                ["pageSize"] = Integer("The default number of items per page. It must not exceed maxPageSize when maxPageSize is greater than zero.", 10, 1),
                ["maxPageSize"] = Integer("The maximum permitted page size. Zero means no explicit maximum.", 100, 0),
                ["maxPagedCount"] = Integer("The maximum number of paged items. Zero means no explicit maximum.", 0, 0),
                ["calendar"] = NullableString("The calendar identifier, or null to use the configured default.", null),
                ["resourceDebugMode"] = Enum("Controls whether debug resource files are used.", "FromConfiguration", "FromConfiguration", "Enabled", "Disabled"),
                ["useCdn"] = Boolean("Whether resources should use the configured CDN.", false),
                ["cdnBaseUrl"] = NullableString("The CDN base URL, or null when no CDN URL is configured.", null),
                ["appendVersion"] = Boolean("Whether a resource version query string is appended to resource URLs.", true),
                ["cacheMode"] = Enum("Controls site resource caching.", "FromConfiguration", "FromConfiguration", "Enabled", "DebugEnabled", "Disabled"),
            },
        };

    private static ProposedSiteSettings CreateProposedSettings(ISite site, SiteSettingsUpdateRequest request)
    {
        var resourceDebugMode = site.ResourceDebugMode;
        var cacheMode = site.CacheMode;

        var resourceDebugModeIsValid = !request.HasResourceDebugMode ||
            TryParseEnumName(request.ResourceDebugMode, out resourceDebugMode);

        var cacheModeIsValid = !request.HasCacheMode ||
            TryParseEnumName(request.CacheMode, out cacheMode);

        return new ProposedSiteSettings
        {
            SiteName = request.HasSiteName ? request.SiteName : site.SiteName,
            PageTitleFormat = request.HasPageTitleFormat ? request.PageTitleFormat : site.PageTitleFormat,
            BaseUrl = request.HasBaseUrl ? request.BaseUrl : site.BaseUrl,
            TimeZoneId = request.HasTimeZoneId ? request.TimeZoneId : site.TimeZoneId,
            PageSize = request.HasPageSize ? request.PageSize : site.PageSize,
            MaxPageSize = request.HasMaxPageSize ? request.MaxPageSize : site.MaxPageSize,
            MaxPagedCount = request.HasMaxPagedCount ? request.MaxPagedCount : site.MaxPagedCount,
            Calendar = request.HasCalendar ? request.Calendar : site.Calendar,
            ResourceDebugMode = resourceDebugMode,
            ResourceDebugModeIsValid = resourceDebugModeIsValid,
            UseCdn = request.HasUseCdn ? request.UseCdn : site.UseCdn,
            CdnBaseUrl = request.HasCdnBaseUrl ? request.CdnBaseUrl : site.CdnBaseUrl,
            AppendVersion = request.HasAppendVersion ? request.AppendVersion : site.AppendVersion,
            CacheMode = cacheMode,
            CacheModeIsValid = cacheModeIsValid,
        };
    }

    private Dictionary<string, string[]> Validate(SiteSettingsUpdateRequest request, ProposedSiteSettings proposed)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (request.HasResourceDebugMode && !proposed.ResourceDebugModeIsValid)
        {
            errors["resourceDebugMode"] = ["Resource debug mode must be one of: FromConfiguration, Enabled, Disabled."];
        }

        if (request.HasCacheMode && !proposed.CacheModeIsValid)
        {
            errors["cacheMode"] = ["Cache mode must be one of: FromConfiguration, Enabled, DebugEnabled, Disabled."];
        }

        AddError("maxPageSize", SiteSettingsValidator.ValidateMaxPageSize(_localizer, proposed.MaxPageSize), errors);
        AddError("maxPagedCount", SiteSettingsValidator.ValidateMaxPagedCount(_localizer, proposed.MaxPagedCount), errors);
        AddError("pageSize", SiteSettingsValidator.ValidatePageSize(_localizer, proposed.PageSize, proposed.MaxPageSize), errors);
        AddError("baseUrl", SiteSettingsValidator.ValidateBaseUrl(_localizer, proposed.BaseUrl), errors);

        return errors;
    }

    private static void AddError(string propertyName, string error, Dictionary<string, string[]> errors)
    {
        if (error is not null)
        {
            errors[propertyName] = [error];
        }
    }

    private static bool Matches(ISite site, ProposedSiteSettings proposed)
        => string.Equals(site.SiteName, proposed.SiteName, StringComparison.Ordinal)
            && string.Equals(site.PageTitleFormat, proposed.PageTitleFormat, StringComparison.Ordinal)
            && string.Equals(site.BaseUrl, proposed.BaseUrl, StringComparison.Ordinal)
            && string.Equals(site.TimeZoneId, proposed.TimeZoneId, StringComparison.Ordinal)
            && site.PageSize == proposed.PageSize
            && site.MaxPageSize == proposed.MaxPageSize
            && site.MaxPagedCount == proposed.MaxPagedCount
            && string.Equals(site.Calendar, proposed.Calendar, StringComparison.Ordinal)
            && site.ResourceDebugMode == proposed.ResourceDebugMode
            && site.UseCdn == proposed.UseCdn
            && string.Equals(site.CdnBaseUrl, proposed.CdnBaseUrl, StringComparison.Ordinal)
            && site.AppendVersion == proposed.AppendVersion
            && site.CacheMode == proposed.CacheMode;

    private static void Apply(ISite site, ProposedSiteSettings proposed)
    {
        site.SiteName = proposed.SiteName;
        site.PageTitleFormat = proposed.PageTitleFormat;
        site.BaseUrl = proposed.BaseUrl;
        site.TimeZoneId = proposed.TimeZoneId;
        site.PageSize = proposed.PageSize;
        site.MaxPageSize = proposed.MaxPageSize;
        site.MaxPagedCount = proposed.MaxPagedCount;
        site.Calendar = proposed.Calendar;
        site.ResourceDebugMode = proposed.ResourceDebugMode;
        site.UseCdn = proposed.UseCdn;
        site.CdnBaseUrl = proposed.CdnBaseUrl;
        site.AppendVersion = proposed.AppendVersion;
        site.CacheMode = proposed.CacheMode;
    }

    private static bool TryParseEnumName<TEnum>(string value, out TEnum result)
        where TEnum : struct, System.Enum
    {
        result = default;

        return value is not null &&
            System.Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase) &&
            System.Enum.TryParse(value, ignoreCase: true, out result);
    }

    private static JsonObject NullableString(string description, string defaultValue)
        => new()
        {
            ["type"] = new JsonArray("string", "null"),
            ["description"] = description,
            ["default"] = defaultValue,
        };

    private static JsonObject BaseUrl()
        => new()
        {
            ["description"] = "The fully qualified public base URL. An empty string or null leaves it unconfigured.",
            ["default"] = null,
            ["anyOf"] = new JsonArray(
                new JsonObject { ["type"] = "null" },
                new JsonObject { ["type"] = "string", ["const"] = string.Empty },
                new JsonObject { ["type"] = "string", ["format"] = "uri" }),
        };

    private static JsonObject Integer(string description, int defaultValue, int minimum)
        => new()
        {
            ["type"] = "integer",
            ["description"] = description,
            ["default"] = defaultValue,
            ["minimum"] = minimum,
        };

    private static JsonObject Boolean(string description, bool defaultValue)
        => new()
        {
            ["type"] = "boolean",
            ["description"] = description,
            ["default"] = defaultValue,
        };

    private static JsonObject Enum(string description, string defaultValue, params string[] values)
        => new()
        {
            ["type"] = "string",
            ["description"] = description,
            ["default"] = defaultValue,
            ["enum"] = new JsonArray(values.Select(value => JsonValue.Create(value)).ToArray()),
        };

    private sealed class ProposedSiteSettings
    {
        public string SiteName { get; init; }
        public string PageTitleFormat { get; init; }
        public string BaseUrl { get; init; }
        public string TimeZoneId { get; init; }
        public int PageSize { get; init; }
        public int MaxPageSize { get; init; }
        public int MaxPagedCount { get; init; }
        public string Calendar { get; init; }
        public ResourceDebugMode ResourceDebugMode { get; init; }
        public bool ResourceDebugModeIsValid { get; init; }
        public bool UseCdn { get; init; }
        public string CdnBaseUrl { get; init; }
        public bool AppendVersion { get; init; }
        public CacheMode CacheMode { get; init; }
        public bool CacheModeIsValid { get; init; }
    }
}

internal sealed class SiteSettingsManagementResult<T>
{
    public bool IsForbidden { get; private init; }
    public T Value { get; private init; }
    public IReadOnlyDictionary<string, string[]> Errors { get; private init; } = new Dictionary<string, string[]>();

    public static SiteSettingsManagementResult<T> Success(T value) => new() { Value = value };

    public static SiteSettingsManagementResult<T> Forbidden() => new() { IsForbidden = true };

    public static SiteSettingsManagementResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) => new() { Errors = errors };
}

internal sealed class SiteSettingsRemoteManagementCapabilityProvider : IRemoteManagementCapabilityProvider
{
    public ValueTask<IEnumerable<RemoteManagementCapability>> GetCapabilitiesAsync() =>
        ValueTask.FromResult<IEnumerable<RemoteManagementCapability>>(
        [
            new RemoteManagementCapability
            {
                Id = SiteSettingsManagementEndpoints.Capability,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Settings",
            },
            new RemoteManagementCapability
            {
                Id = SiteSettingsSectionEndpoints.Capability,
                Version = $"{RemoteManagementConstants.ProtocolMajorVersion}.{RemoteManagementConstants.ProtocolMinorVersion}",
                DisplayName = "Settings Sections",
            },
        ]);
}
