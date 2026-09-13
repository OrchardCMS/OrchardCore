using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Endpoints;

internal static class LocalizationManagementEndpoints
{
    internal const string Capability = "localization";

    public static void AddLocalizationManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/localization").WithTags("Localization")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api })
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .DisableAntiforgery();

        group.MapGet("/cultures", ListAsync)
            .WithName("ApiListLocalizationCultures")
            .WithSummary("Lists tenant cultures.")
            .WithDescription("Lists supported cultures, or all available cultures when includeAvailable is true, with default and supported flags. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "cultures"], "list")
            {
                Capability = Capability,
                TableColumns = { new("items[].name", "Culture"), new("items[].displayName", "Name"), new("items[].isSupported", "Supported"), new("items[].isDefault", "Default") },
            })
            .Produces<CultureListResponse>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
        group.MapGet("/cultures/available", AvailableCulturesAsync)
            .WithName("ApiListAvailableLocalizationCultures").WithSummary("Lists cultures available on the server.")
            .WithDescription("Lists available culture names with supported and default flags. Use a name with localization cultures add. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "cultures"], "available")
            {
                Capability = Capability,
                TableColumns = { new("items[].name", "Culture"), new("items[].displayName", "Name"), new("items[].isSupported", "Supported"), new("items[].isDefault", "Default") },
            })
            .Produces<CultureListResponse>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
        group.MapPut("/cultures/{culture}", AddCultureAsync)
            .WithName("ApiAddLocalizationCulture").WithSummary("Adds a supported culture.")
            .WithDescription("Adds one available culture, preserving the default culture, other supported cultures and fallback setting. Repeating an addition is harmless. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "cultures"], "add") { Capability = Capability, Arguments = { new("culture", 0) } })
            .Produces<CultureSettings>().ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403);
        group.MapDelete("/cultures/{culture}", RemoveCultureAsync)
            .WithName("ApiRemoveLocalizationCulture").WithSummary("Removes a supported culture.")
            .WithDescription("Removes one supported culture without deleting translations. The default culture cannot be removed; change it with localization settings update first. Repeating a removal is harmless. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "cultures"], "remove") { Capability = Capability, RequiresConfirmation = true, Arguments = { new("culture", 0) } })
            .Produces<CultureSettings>().ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403);
        group.MapGet("/settings", GetAsync)
            .WithName("ApiGetLocalizationSettings").WithSummary("Shows culture settings.")
            .WithDescription("Returns the default culture, supported cultures and parent-culture fallback. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "settings"], "show") { Capability = Capability })
            .Produces<CultureSettings>().ProducesProblem(401).ProducesProblem(403);
        group.MapPut("/settings", UpdateAsync)
            .WithName("ApiUpdateLocalizationSettings").WithSummary("Updates culture settings.")
            .WithDescription("Replaces culture settings and releases the tenant when settings change. The default culture must be supported. Identical updates do not release the tenant. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "settings"], "update") { Capability = Capability, InputMode = CliInputMode.Json })
            .Accepts<CultureSettings>("application/json")
            .Produces<CultureSettings>().ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403);
        group.MapGet("/strings", StringGroupsAsync)
            .WithName("ApiListLocalizationStringGroups").WithSummary("Lists available UI string groups.")
            .WithDescription("Lists group names advertised by enabled JavaScript localization providers. Use a returned name with GET /api/localization/strings/{groupName}. Requires ManageCultures.")
            .Produces<UiStringGroupsResponse>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
        group.MapGet("/strings/{groupName}", StringsAsync)
            .WithName("ApiGetLocalizationStrings").WithSummary("Shows translated UI strings.")
            .WithDescription("Reads a registered JavaScript localization group in a supported culture. Does not edit PO files. Untranslated strings retain their source text. Requires ManageCultures.")
            .Produces<UiStringsResponse>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization, [FromServices] ILocalizationService localization)
    {
        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        return TypedResults.Ok(await ReadAsync(localization));
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization, [FromServices] ILocalizationService localization, [AsParameters] CultureListRequest request)
    {
        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizationManagementEndpoints));

        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take < 1 || take > 200)
        {
            return context.ApiBadRequestProblem(detail: S["Skip must be nonnegative and take must be between 1 and 200."]);
        }

        var settings = await ReadAsync(localization);
        var items = localization.GetAllCulturesAndAliases()
            .DistinctBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
            .Select(culture => new CultureResponse
            {
                Name = culture.Name, DisplayName = culture.DisplayName,
                IsSupported = settings.SupportedCultures.Contains(culture.Name, StringComparer.OrdinalIgnoreCase),
                IsDefault = string.Equals(settings.DefaultCulture, culture.Name, StringComparison.OrdinalIgnoreCase),
            })
            .Where(culture => request.IncludeAvailable == true || culture.IsSupported)
            .OrderBy(culture => culture.Name, StringComparer.Ordinal).ToArray();
        return TypedResults.Ok(new CultureListResponse { Skip = skip, Take = take, TotalCount = items.Length, Items = items.Skip(skip).Take(take).ToArray() });
    }

    internal static Task<IResult> AvailableCulturesAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILocalizationService localization, [AsParameters] LocalizationListRequest request)
        => ListAsync(context, authorization, localization, new CultureListRequest { IncludeAvailable = true, Skip = request.Skip, Take = request.Take });

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization, [FromServices] ILocalizationService localization,
        [FromServices] ISiteService siteService, [FromServices] IShellReleaseManager release, [FromBody] CultureSettings request)
    {
        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizationManagementEndpoints));

        var available = localization.GetAllCulturesAndAliases().Select(culture => culture.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToDictionary(name => name, StringComparer.OrdinalIgnoreCase);
        if (request?.SupportedCultures is not { Length: > 0 and <= 1000 }
            || request.DefaultCulture is null || !available.TryGetValue(request.DefaultCulture, out var defaultCulture)
            || request.SupportedCultures.Any(name => name is null || !available.ContainsKey(name)))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["cultures"] = [S["Provide a valid default culture and between 1 and 1000 available supported cultures. Use localization cultures available to discover names."]] });
        }

        var supported = request.SupportedCultures.Select(name => available[name]).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray();
        if (!supported.Contains(defaultCulture, StringComparer.OrdinalIgnoreCase))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["defaultCulture"] = [S["The default culture must be included in supportedCultures."]] });
        }

        var result = new CultureSettings { DefaultCulture = defaultCulture, SupportedCultures = supported, FallBackToParentCulture = request.FallBackToParentCulture };
        var current = await ReadAsync(localization);
        if (!string.Equals(current.DefaultCulture, result.DefaultCulture, StringComparison.OrdinalIgnoreCase)
            || current.FallBackToParentCulture != result.FallBackToParentCulture
            || !current.SupportedCultures.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(supported))
        {
            var site = await siteService.LoadSiteSettingsAsync();
            site.Put(new LocalizationSettings { DefaultCulture = result.DefaultCulture, SupportedCultures = supported, FallBackToParentCulture = result.FallBackToParentCulture });
            await siteService.UpdateSiteSettingsAsync(site);
            release.RequestRelease();
        }

        return TypedResults.Ok(result);
    }

    internal static Task<IResult> AddCultureAsync(HttpContext context, [FromServices] IAuthorizationService authorization, [FromServices] ILocalizationService localization,
        [FromServices] ISiteService siteService, [FromServices] IShellReleaseManager release, string culture)
        => ChangeCultureAsync(context, authorization, localization, siteService, release, culture, remove: false);

    internal static Task<IResult> RemoveCultureAsync(HttpContext context, [FromServices] IAuthorizationService authorization, [FromServices] ILocalizationService localization,
        [FromServices] ISiteService siteService, [FromServices] IShellReleaseManager release, string culture)
        => ChangeCultureAsync(context, authorization, localization, siteService, release, culture, remove: true);

    private static async Task<IResult> ChangeCultureAsync(HttpContext context, IAuthorizationService authorization, ILocalizationService localization,
        ISiteService siteService, IShellReleaseManager release, string culture, bool remove)
    {
        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizationManagementEndpoints));
        var name = localization.GetAllCulturesAndAliases().FirstOrDefault(item => string.Equals(item.Name, culture, StringComparison.OrdinalIgnoreCase))?.Name;
        if (name is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["culture"] = [S["Use an available culture from localization cultures available."]] });
        }

        var current = await ReadAsync(localization);
        if (remove && string.Equals(current.DefaultCulture, name, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["culture"] = [S["The default culture cannot be removed. Change the default with localization settings update first."]] });
        }

        return await UpdateAsync(context, authorization, localization, siteService, release, new CultureSettings
        {
            DefaultCulture = current.DefaultCulture,
            SupportedCultures = remove
                ? current.SupportedCultures.Where(item => !string.Equals(item, name, StringComparison.OrdinalIgnoreCase)).ToArray()
                : [.. current.SupportedCultures, name],
            FallBackToParentCulture = current.FallBackToParentCulture,
        });
    }

    internal static async Task<IResult> StringGroupsAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IEnumerable<IJSLocalizer> localizers, [AsParameters] LocalizationListRequest request)
    {
        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take < 1 || take > 200)
        {
            var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizationManagementEndpoints));
            return context.ApiBadRequestProblem(detail: S["Skip must be nonnegative and take must be between 1 and 200."]);
        }

        var groups = localizers.SelectMany(localizer => localizer.GetLocalizationGroups())
            .Where(name => !string.IsNullOrWhiteSpace(name) && name.Length <= 200)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        return TypedResults.Ok(new UiStringGroupsResponse
        {
            Skip = skip, Take = take, TotalCount = groups.Length,
            Items = groups.Skip(skip).Take(take).Select(name => new UiStringGroup { Name = name }).ToArray(),
        });
    }

    internal static async Task<IResult> StringsAsync(HttpContext context, [FromServices] IAuthorizationService authorization, [FromServices] ILocalizationService localization,
        [FromServices] IEnumerable<IJSLocalizer> localizers, string groupName, [AsParameters] UiStringsRequest request)
    {
        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizationManagementEndpoints));

        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var culture = request.Culture ?? await localization.GetDefaultCultureAsync();
        var supported = (await localization.GetSupportedCulturesAsync()).FirstOrDefault(name => string.Equals(name, culture, StringComparison.OrdinalIgnoreCase));
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (supported is null || groupName.Length > 200 || skip < 0 || take < 1 || take > 200)
        {
            return context.ApiBadRequestProblem(detail: S["Use a supported culture, a group name of at most 200 characters, nonnegative skip and take between 1 and 200."]);
        }

        using var scope = CultureScope.Create(supported, ignoreSystemSettings: true);
        var strings = localizers.GetMergedLocalizations(groupName);
        if (strings.Count == 0)
        {
            return context.ApiNotFoundProblem(detail: S["No UI strings are registered for this group."]);
        }

        var items = strings.OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => new UiString { Key = item.Key, Value = item.Value }).ToArray();
        return TypedResults.Ok(new UiStringsResponse { Group = groupName, Culture = supported, Skip = skip, Take = take, TotalCount = items.Length, Items = items.Skip(skip).Take(take).ToArray() });
    }

    private static async Task<CultureSettings> ReadAsync(ILocalizationService localization) => new()
    {
        DefaultCulture = await localization.GetDefaultCultureAsync(),
        SupportedCultures = await localization.GetSupportedCulturesAsync(),
        FallBackToParentCulture = localization.FallBackToParentCultures,
    };

#nullable enable
    internal sealed class CultureSettings
    {
        [Required(AllowEmptyStrings = true), Description("Default culture, which must also occur in supportedCultures. Empty string denotes invariant culture.")]
        public string DefaultCulture { get; init; } = null!;

        [Required, MinLength(1), MaxLength(1000), Description("Complete set of supported culture names. Discover names with localization cultures available.")]
        public string[] SupportedCultures { get; init; } = null!;

        [Description("Whether requests may fall back to a supported parent culture. Defaults to false when omitted.")]
        public bool FallBackToParentCulture { get; init; }
    }

    internal sealed class CultureListRequest
    {
        [FromQuery, Description("Include cultures available on the server but not enabled for this tenant.")]
        public bool? IncludeAvailable { get; init; }

        [FromQuery]
        public int? Skip { get; init; }

        [FromQuery]
        public int? Take { get; init; }
    }

    internal sealed class LocalizationListRequest
    {
        [FromQuery]
        public int? Skip { get; init; }

        [FromQuery]
        public int? Take { get; init; }
    }

    internal sealed class UiStringGroup
    {
        public string Name { get; init; } = null!;
    }

    internal sealed class UiStringGroupsResponse
    {
        public int Skip { get; init; }

        public int Take { get; init; }

        public int TotalCount { get; init; }

        public UiStringGroup[] Items { get; init; } = [];
    }

    internal sealed class UiStringsRequest
    {
        [FromQuery, Description("Supported culture to use. Defaults to the tenant default culture.")]
        public string? Culture { get; init; }

        [FromQuery]
        public int? Skip { get; init; }

        [FromQuery]
        public int? Take { get; init; }
    }

    internal sealed class CultureResponse
    {
        public string Name { get; init; } = null!;

        public string DisplayName { get; init; } = null!;

        public bool IsSupported { get; init; }

        public bool IsDefault { get; init; }
    }

    internal sealed class CultureListResponse
    {
        public int Skip { get; init; }

        public int Take { get; init; }

        public int TotalCount { get; init; }

        public CultureResponse[] Items { get; init; } = [];
    }

    internal sealed class UiString
    {
        public string Key { get; init; } = null!;

        public string Value { get; init; } = null!;
    }

    internal sealed class UiStringsResponse
    {
        public string Group { get; init; } = null!;

        public string Culture { get; init; } = null!;

        public int Skip { get; init; }

        public int Take { get; init; }

        public int TotalCount { get; init; }

        public UiString[] Items { get; init; } = [];
    }
}
