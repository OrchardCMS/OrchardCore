using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Models;
using OrchardCore.Features.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Features.Endpoints.Management;

internal static class FeatureManagementEndpoints
{
    private const string RoutePrefix = "api/features";

    public static IEndpointRouteBuilder AddFeatureManagementEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGet(RoutePrefix, ListAsync)
            .WithName("ApiListFeatures")
            .WithSummary("Lists features.")
            .WithDescription("Returns available non-theme features with their enablement state and dependency information.")
            .WithCliCommand(new CliOperationMetadata(["features"], "list")
            {
                Capability = FeatureManagementApiEndpointConventions.CapabilityName,
                TableColumns =
                {
                    new CliTableColumnMetadata("items.id", "Id"),
                    new CliTableColumnMetadata("items.name", "Name"),
                    new CliTableColumnMetadata("items.category", "Category"),
                    new CliTableColumnMetadata("items.isEnabled", "Enabled"),
                },
            })
            .Produces<FeatureListResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        builder.MapManagementGet(RoutePrefix + "/{featureId}", GetAsync)
            .WithName("ApiGetFeature")
            .WithSummary("Gets a feature.")
            .WithDescription("Returns a single feature with dependency and state details.")
            .WithCliCommand(new CliOperationMetadata(["features"], "show")
            {
                Capability = FeatureManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("featureId", 0) },
            })
            .Produces<FeatureResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix + "/{featureId}:enable", EnableAsync)
            .WithName("ApiEnableFeature")
            .WithSummary("Enables a feature.")
            .WithDescription("Enables a feature by using the existing shell feature manager dependency resolution.")
            .WithCliCommand(new CliOperationMetadata(["features"], "enable")
            {
                Capability = FeatureManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("featureId", 0) },
            })
            .Produces<FeatureResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        builder.MapManagementPost(RoutePrefix + "/{featureId}:disable", DisableAsync)
            .WithName("ApiDisableFeature")
            .WithSummary("Disables a feature.")
            .WithDescription("Disables a feature by using the existing feature manager dependency checks.")
            .WithCliCommand(new CliOperationMetadata(["features"], "disable")
            {
                Capability = FeatureManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("featureId", 0) },
                RequiresConfirmation = true,
            })
            .Produces<FeatureResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    internal static async Task<IResult> ListAsync(HttpContext httpContext, IAuthorizationService authorizationService, [FromServices] FeatureService featureService, [AsParameters] FeatureListRequest request)
    {
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;

        if (ValidatePaging(skip, take) is { } pagingError)
        {
            return pagingError;
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, FeaturesPermissions.ManageFeatures))
        {
            return httpContext.ApiForbidProblem();
        }

        var items = (await featureService.GetModuleFeaturesAsync())
            .Where(feature => MatchesFilter(feature, request))
            .OrderBy(feature => feature.Descriptor.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(feature => feature.Descriptor.Id, StringComparer.OrdinalIgnoreCase)
            .Select(feature => ToResponse(feature))
            .ToArray();

        return TypedResults.Ok(new FeatureListResponse
        {
            Skip = skip,
            Take = take,
            TotalCount = items.Length,
            Items = items.Skip(skip).Take(take).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext httpContext, string featureId, IAuthorizationService authorizationService, [FromServices] FeatureService featureService, IStringLocalizer<Permissions> localizer)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, FeaturesPermissions.ManageFeatures))
        {
            return httpContext.ApiForbidProblem();
        }

        var feature = (await featureService.GetModuleFeaturesAsync()).FirstOrDefault(item => string.Equals(item.Descriptor.Id, featureId, StringComparison.OrdinalIgnoreCase));
        if (feature is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Feature not found: '{0}'.", featureId]);
        }

        return TypedResults.Ok(ToResponse(feature));
    }

    internal static async Task<IResult> EnableAsync(HttpContext httpContext, string featureId, IAuthorizationService authorizationService, [FromServices] FeatureService featureService, [FromServices] IShellFeaturesManager shellFeaturesManager, IStringLocalizer<Permissions> localizer, bool force = false)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, FeaturesPermissions.ManageFeatures))
        {
            return httpContext.ApiForbidProblem();
        }

        var feature = await featureService.GetAvailableFeature(featureId);
        if (feature is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Feature not found: '{0}'.", featureId]);
        }

        var wasEnabled = (await shellFeaturesManager.GetEnabledFeaturesAsync()).Any(candidate => candidate.Id == feature.Id);
        var enabledFeatures = await shellFeaturesManager.EnableFeaturesAsync([feature], force);
        if (!wasEnabled && !enabledFeatures.Any(candidate => candidate.Id == feature.Id))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["The feature '{0}' could not be enabled. Check its dependencies and the active feature profile.", featureId]);
        }

        var updated = (await featureService.GetModuleFeaturesAsync()).First(item => string.Equals(item.Descriptor.Id, featureId, StringComparison.OrdinalIgnoreCase));
        return TypedResults.Ok(ToResponse(updated, isEnabled: true));
    }

    internal static async Task<IResult> DisableAsync(HttpContext httpContext, string featureId, IAuthorizationService authorizationService, [FromServices] FeatureService featureService, [FromServices] IShellFeaturesManager shellFeaturesManager, IStringLocalizer<Permissions> localizer, bool force = false)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, FeaturesPermissions.ManageFeatures))
        {
            return httpContext.ApiForbidProblem();
        }

        var feature = await featureService.GetAvailableFeature(featureId);
        if (feature is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Feature not found: '{0}'.", featureId]);
        }

        if (feature.IsAlwaysEnabled)
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["You can only disable features that are not always enabled."]);
        }

        var wasEnabled = (await shellFeaturesManager.GetEnabledFeaturesAsync()).Any(candidate => candidate.Id == feature.Id);
        var disabledFeatures = await shellFeaturesManager.DisableFeaturesAsync([feature], force);
        if (wasEnabled && !disabledFeatures.Any(candidate => candidate.Id == feature.Id))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["The feature '{0}' could not be disabled. Check its dependents and the active feature profile.", featureId]);
        }

        var updated = (await featureService.GetModuleFeaturesAsync()).First(item => string.Equals(item.Descriptor.Id, featureId, StringComparison.OrdinalIgnoreCase));
        return TypedResults.Ok(ToResponse(updated, isEnabled: false));
    }

    internal static FeatureResponse ToResponse(ModuleFeature feature, bool? isEnabled = null)
        => new()
        {
            Id = feature.Descriptor.Id,
            Name = feature.Descriptor.Name,
            Category = feature.Descriptor.Category,
            Description = feature.Descriptor.Description,
            IsEnabled = isEnabled ?? feature.IsEnabled,
            IsAlwaysEnabled = feature.IsAlwaysEnabled,
            EnabledByDependencyOnly = feature.EnabledByDependencyOnly,
            Dependencies = feature.FeatureDependencies.Select(dependency => dependency.Id).OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToArray(),
            EnabledDependents = feature.EnabledDependentFeatures.Select(dependent => dependent.Id).OrderBy(id => id, StringComparer.OrdinalIgnoreCase).ToArray(),
            DefaultTenantOnly = feature.Descriptor.DefaultTenantOnly,
            ExtensionId = feature.Descriptor.Extension.Id,
            ExtensionName = feature.Descriptor.Extension.Manifest.Name,
        };

    private static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(title: "Bad request", detail: "Skip must be zero or greater and take must be greater than zero.", statusCode: StatusCodes.Status400BadRequest);
        }

        if (take > 200)
        {
            return TypedResults.Problem(title: "Bad request", detail: "Take cannot exceed 200.", statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
    }

    private static bool MatchesFilter(ModuleFeature feature, FeatureListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search) &&
            !feature.Descriptor.Id.Contains(request.Search, StringComparison.OrdinalIgnoreCase) &&
            !feature.Descriptor.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) &&
            !(feature.Descriptor.Description?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Category) && !string.Equals(feature.Descriptor.Category, request.Category, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (request.Enabled.HasValue && feature.IsEnabled != request.Enabled.Value)
        {
            return false;
        }

        return true;
    }

    internal sealed class FeatureListRequest
    {
        public int? Skip { get; init; }
        public int? Take { get; init; }
        public string Search { get; init; }
        public string Category { get; init; }
        public bool? Enabled { get; init; }
    }

    internal sealed class FeatureListResponse
    {
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public FeatureResponse[] Items { get; init; } = [];
    }

    internal sealed class FeatureResponse
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; }
        public string Category { get; init; }
        public string Description { get; init; }
        public bool IsEnabled { get; init; }
        public bool IsAlwaysEnabled { get; init; }
        public bool EnabledByDependencyOnly { get; init; }
        public bool DefaultTenantOnly { get; init; }
        public string ExtensionId { get; init; }
        public string ExtensionName { get; init; }
        public string[] Dependencies { get; init; } = [];
        public string[] EnabledDependents { get; init; } = [];
    }
}

internal static class FeatureManagementApiEndpointConventions
{
    public const string CapabilityName = "features";
    public const string TagName = "Features";

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
