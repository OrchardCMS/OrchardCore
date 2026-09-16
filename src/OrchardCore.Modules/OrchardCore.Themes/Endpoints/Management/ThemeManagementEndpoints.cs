using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules.Manifest;
using OrchardCore.RemoteManagement;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes.Endpoints.Management;

internal static class ThemeManagementEndpoints
{
    private const string RoutePrefix = "api/themes";
    private const int DefaultTake = 50;
    private const int MaximumTake = 200;

    public static IEndpointRouteBuilder AddThemeManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGet(RoutePrefix, ListAsync)
            .WithName("ApiListThemes")
            .WithSummary("Lists themes.")
            .WithDescription("Returns available site and admin themes with their enablement and current-selection state.")
            .WithCliCommand(new CliOperationMetadata(["themes"], "list")
            {
                Capability = ThemeManagementEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items[].id", "Id"),
                    new CliTableColumnMetadata("items[].name", "Name"),
                    new CliTableColumnMetadata("items[].isAdmin", "Admin"),
                    new CliTableColumnMetadata("items[].isEnabled", "Enabled"),
                    new CliTableColumnMetadata("items[].isCurrent", "Current"),
                },
            })
            .Produces<ThemeListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementPost(RoutePrefix + "/{themeId}:enable", EnableAsync)
            .WithName("ApiEnableTheme")
            .WithSummary("Enables a theme.")
            .WithDescription("Enables a theme and its base themes. Repeating the operation for an enabled theme returns its current state.")
            .WithCliCommand(new CliOperationMetadata(["themes"], "enable")
            {
                Capability = ThemeManagementEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("themeId", 0) },
            })
            .Produces<ThemeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix + "/{themeId}:set-current", SetCurrentAsync)
            .WithName("ApiSetCurrentTheme")
            .WithSummary("Sets the current theme.")
            .WithDescription("Selects a site or admin theme according to its manifest type and enables it when necessary.")
            .WithCliCommand(new CliOperationMetadata(["themes"], "set-current")
            {
                Capability = ThemeManagementEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("themeId", 0) },
            })
            .Produces<ThemeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    internal static async Task<IResult> ListAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShellFeaturesManager shellFeaturesManager,
        [FromServices] ISiteThemeService siteThemeService,
        [FromServices] IAdminThemeService adminThemeService,
        [AsParameters] ThemeListRequest request)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ApplyTheme))
        {
            return httpContext.ApiForbidProblem();
        }

        var skip = request.Skip ?? 0;
        var take = request.Take ?? DefaultTake;
        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        var enabledThemeIds = (await shellFeaturesManager.GetEnabledFeaturesAsync())
            .Select(feature => feature.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentSiteTheme = await siteThemeService.GetSiteThemeNameAsync();
        var currentAdminTheme = await adminThemeService.GetAdminThemeNameAsync();
        var themes = (await shellFeaturesManager.GetAvailableFeaturesAsync())
            .Where(IsManageableTheme)
            .Select(feature => ToResponse(feature, enabledThemeIds.Contains(feature.Id), currentSiteTheme, currentAdminTheme))
            .Where(theme => MatchesFilter(theme, request))
            .OrderByDescending(theme => theme.IsCurrent)
            .ThenBy(theme => theme.IsAdmin)
            .ThenBy(theme => theme.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(theme => theme.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return TypedResults.Ok(new ThemeListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = themes.Length,
            Items = themes.Skip(skip).Take(take).ToArray(),
        });
    }

    internal static async Task<IResult> EnableAsync(
        HttpContext httpContext,
        string themeId,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShellFeaturesManager shellFeaturesManager,
        [FromServices] ISiteThemeService siteThemeService,
        [FromServices] IAdminThemeService adminThemeService,
        [FromServices] IStringLocalizer<Permissions> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ApplyTheme))
        {
            return httpContext.ApiForbidProblem();
        }

        var theme = await FindThemeAsync(shellFeaturesManager, themeId);
        if (theme is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Theme not found: '{0}'.", themeId]);
        }

        var enabled = (await shellFeaturesManager.GetEnabledFeaturesAsync())
            .Any(feature => string.Equals(feature.Id, theme.Id, StringComparison.OrdinalIgnoreCase));
        var currentSiteTheme = await siteThemeService.GetSiteThemeNameAsync();
        var currentAdminTheme = await adminThemeService.GetAdminThemeNameAsync();
        if (!enabled)
        {
            var enabledFeatures = await shellFeaturesManager.EnableFeaturesAsync([theme], force: true);
            enabled = enabledFeatures.Any(feature => string.Equals(feature.Id, theme.Id, StringComparison.OrdinalIgnoreCase));
            if (!enabled)
            {
                return TypedResults.Problem(
                    title: localizer["Bad request"],
                    detail: localizer["Theme '{0}' could not be enabled. Check its base theme and the active feature profile.", theme.Id],
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        return TypedResults.Ok(ToResponse(theme, enabled, currentSiteTheme, currentAdminTheme));
    }

    internal static async Task<IResult> SetCurrentAsync(
        HttpContext httpContext,
        string themeId,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IShellFeaturesManager shellFeaturesManager,
        [FromServices] ISiteThemeService siteThemeService,
        [FromServices] IAdminThemeService adminThemeService,
        [FromServices] IShellHost shellHost,
        [FromServices] ShellSettings shellSettings,
        [FromServices] IStringLocalizer<Permissions> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.ApplyTheme))
        {
            return httpContext.ApiForbidProblem();
        }

        var theme = await FindThemeAsync(shellFeaturesManager, themeId);
        if (theme is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Theme not found: '{0}'.", themeId]);
        }

        var isAdmin = IsAdminTheme(theme);
        var currentSiteTheme = await siteThemeService.GetSiteThemeNameAsync();
        var currentAdminTheme = await adminThemeService.GetAdminThemeNameAsync();
        var isCurrent = string.Equals(isAdmin ? currentAdminTheme : currentSiteTheme, theme.Id, StringComparison.Ordinal);
        var enabled = (await shellFeaturesManager.GetEnabledFeaturesAsync())
            .Any(feature => string.Equals(feature.Id, theme.Id, StringComparison.OrdinalIgnoreCase));
        var wasEnabled = enabled;
        if (!enabled)
        {
            var enabledFeatures = await shellFeaturesManager.EnableFeaturesAsync([theme], force: true);
            enabled = enabledFeatures.Any(feature => string.Equals(feature.Id, theme.Id, StringComparison.OrdinalIgnoreCase));
            if (!enabled)
            {
                return TypedResults.Problem(
                    title: localizer["Bad request"],
                    detail: localizer["Theme '{0}' could not be enabled. Check its base theme and the active feature profile.", theme.Id],
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        if (!isCurrent)
        {
            if (wasEnabled)
            {
                if (isAdmin)
                {
                    await adminThemeService.SetAdminThemeAsync(theme.Id);
                    currentAdminTheme = theme.Id;
                }
                else
                {
                    await siteThemeService.SetSiteThemeAsync(theme.Id);
                    currentSiteTheme = theme.Id;
                }
            }
            else
            {
                var scope = await shellHost.GetScopeAsync(shellSettings);
                await scope.UsingAsync(async childScope =>
                {
                    if (isAdmin)
                    {
                        await childScope.ServiceProvider.GetRequiredService<IAdminThemeService>().SetAdminThemeAsync(theme.Id);
                    }
                    else
                    {
                        await childScope.ServiceProvider.GetRequiredService<ISiteThemeService>().SetSiteThemeAsync(theme.Id);
                    }
                }, activateShell: false);
            }

            if (isAdmin)
            {
                currentAdminTheme = theme.Id;
            }
            else
            {
                currentSiteTheme = theme.Id;
            }
        }

        return TypedResults.Ok(ToResponse(theme, enabled, currentSiteTheme, currentAdminTheme));
    }

    private static async Task<IFeatureInfo> FindThemeAsync(IShellFeaturesManager shellFeaturesManager, string themeId)
        => (await shellFeaturesManager.GetAvailableFeaturesAsync())
            .FirstOrDefault(feature =>
                string.Equals(feature.Id, themeId, StringComparison.OrdinalIgnoreCase) &&
                IsManageableTheme(feature));

    internal static bool IsManageableTheme(IFeatureInfo feature)
        => feature.IsTheme() &&
            !feature.IsAlwaysEnabled &&
            !feature.EnabledByDependencyOnly &&
            !feature.Extension.Manifest.Tags.Any(tag => string.Equals(tag, "hidden", StringComparison.OrdinalIgnoreCase));

    internal static ThemeResponse ToResponse(
        IFeatureInfo feature,
        bool isEnabled,
        string currentSiteTheme,
        string currentAdminTheme)
    {
        var isAdmin = IsAdminTheme(feature);

        return new ThemeResponse
        {
            Id = feature.Id,
            Name = feature.Name ?? feature.Id,
            Description = feature.Description,
            IsAdmin = isAdmin,
            IsEnabled = isEnabled,
            IsCurrent = string.Equals(isAdmin ? currentAdminTheme : currentSiteTheme, feature.Id, StringComparison.Ordinal),
        };
    }

    private static bool IsAdminTheme(IFeatureInfo feature)
        => feature.Extension.Manifest.Tags.Any(tag => string.Equals(tag, ManifestConstants.AdminTag, StringComparison.OrdinalIgnoreCase));

    private static bool MatchesFilter(ThemeResponse theme, ThemeListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search) &&
            !theme.Id.Contains(request.Search, StringComparison.OrdinalIgnoreCase) &&
            !theme.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) &&
            !(theme.Description?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        return (!request.Admin.HasValue || request.Admin.Value == theme.IsAdmin) &&
            (!request.Enabled.HasValue || request.Enabled.Value == theme.IsEnabled) &&
            (!request.Current.HasValue || request.Current.Value == theme.IsCurrent);
    }

    private static ProblemHttpResult ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(
                title: "Bad request",
                detail: "Skip must be zero or greater and take must be greater than zero.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > MaximumTake)
        {
            return TypedResults.Problem(
                title: "Bad request",
                detail: $"Take cannot exceed {MaximumTake}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
    }

    internal sealed class ThemeListRequest
    {
        public int? Skip { get; init; }
        public int? Take { get; init; }
        public string Search { get; init; }
        public bool? Admin { get; init; }
        public bool? Enabled { get; init; }
        public bool? Current { get; init; }
    }

    internal sealed class ThemeListResponse
    {
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public ThemeResponse[] Items { get; init; } = [];
    }

    internal sealed class ThemeResponse
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public bool IsAdmin { get; init; }
        public bool IsEnabled { get; init; }
        public bool IsCurrent { get; init; }
    }
}

internal static class ThemeManagementEndpointConventions
{
    public const string CapabilityName = "themes";
    public const string TagName = "Themes";

    public static RouteHandlerBuilder MapManagementGet(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapGet(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    public static RouteHandlerBuilder MapManagementPost(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPost(pattern, handler)
            .WithTags(TagName)
            .DisableAntiforgery()
            .RequireAuthorization(CreateBearerPolicy());

    private static Action<AuthorizationPolicyBuilder> CreateBearerPolicy()
        => static policy => policy
            .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
            .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement))
            .RequireAuthenticatedUser();
}
