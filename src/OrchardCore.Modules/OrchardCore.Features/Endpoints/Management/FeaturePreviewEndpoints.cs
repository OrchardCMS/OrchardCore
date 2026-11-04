using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Features.Endpoints.Management;

internal static class FeaturePreviewEndpoints
{
    public static IEndpointRouteBuilder AddFeaturePreviewEndpoints(this IEndpointRouteBuilder builder)
    {
        var routes = builder.MapManagementGroup(FeatureManagementApiEndpointConventions.TagName);

        routes.MapGet("api/features/{featureId}/preview", PreviewAsync)
            .WithName("ApiPreviewFeatureChange")
            .WithSummary("Previews enabling or disabling a feature.")
            .WithDescription("Returns the features an enable or a disable would change, without changing anything. It follows the dependency resolution of the shell feature manager.")
            .WithCliCommand(new CliOperationMetadata(["features"], "preview")
            {
                Capability = FeatureManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("featureId", 0) },
            })
            .Produces<FeaturePreviewResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    internal static async Task<IResult> PreviewAsync(
        HttpContext httpContext,
        string featureId,
        IAuthorizationService authorizationService,
        [FromServices] FeatureService featureService,
        [FromServices] IShellFeaturesManager shellFeaturesManager,
        [FromServices] IExtensionManager extensionManager,
        IStringLocalizer<Permissions> localizer,
        [FromQuery] string action,
        [FromQuery] bool force = false)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, FeaturesPermissions.ManageFeatures))
        {
            return httpContext.ApiForbidProblem();
        }

        var enabling = string.Equals(action, "enable", StringComparison.OrdinalIgnoreCase);
        if (!enabling && !string.Equals(action, "disable", StringComparison.OrdinalIgnoreCase))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["The action must be 'enable' or 'disable'."]);
        }

        var feature = await featureService.GetAvailableFeature(featureId);
        if (feature is null)
        {
            return httpContext.ApiNotFoundProblem(detail: localizer["Feature not found: '{0}'.", featureId]);
        }

        var enabledFeatures = (await shellFeaturesManager.GetEnabledFeaturesAsync()).ToArray();
        var enabledIds = enabledFeatures.Select(enabled => enabled.Id).ToHashSet(StringComparer.Ordinal);
        var isEnabled = enabledIds.Contains(feature.Id);

        string blockedReason = null;
        IFeatureInfo[] toEnable = [];
        IFeatureInfo[] toDisable = [];

        if (enabling)
        {
            if (feature.EnabledByDependencyOnly && !isEnabled)
            {
                blockedReason = localizer["This feature is only enabled as a dependency of another feature."];
            }
            else
            {
                toEnable = extensionManager.GetFeatureDependencies(feature.Id).Where(candidate => !enabledIds.Contains(candidate.Id)).ToArray();
            }
        }
        else if ((await shellFeaturesManager.GetAlwaysEnabledFeaturesAsync()).Any(candidate => candidate.Id == feature.Id))
        {
            blockedReason = localizer["You can only disable features that are not always enabled."];
        }
        else if (feature.EnabledByDependencyOnly && isEnabled)
        {
            blockedReason = localizer["This feature is only disabled when no enabled feature depends on it anymore."];
        }
        else
        {
            toDisable = extensionManager.GetDependentFeatures(feature.Id).Where(candidate => enabledIds.Contains(candidate.Id)).Reverse().ToArray();
        }

        // The feature manager changes nothing without 'force' when other features have to change along with this one.
        var requiresForce = toEnable.Length > 1 || toDisable.Length > 1;
        var isAllowed = blockedReason is null && (force || !requiresForce);

        return TypedResults.Ok(new FeaturePreviewResponse
        {
            FeatureId = feature.Id,
            Action = enabling ? "enable" : "disable",
            IsEnabled = isEnabled,
            IsAllowed = isAllowed,
            RequiresForce = requiresForce,
            BlockedReason = blockedReason,
            FeaturesToEnable = toEnable.Select(candidate => candidate.Id).ToArray(),
            FeaturesToDisable = toDisable.Select(candidate => candidate.Id).ToArray(),
            DependencyOnlyFeaturesToDisable = GetUnneededDependencyOnlyFeatures(extensionManager, enabledFeatures, toEnable, toDisable),
        });
    }

    // A feature enabled by dependency only is disabled as soon as no remaining feature depends on it.
    private static string[] GetUnneededDependencyOnlyFeatures(IExtensionManager extensionManager, IFeatureInfo[] enabledFeatures, IFeatureInfo[] toEnable, IFeatureInfo[] toDisable)
    {
        var remaining = enabledFeatures.Except(toDisable).Concat(toEnable).ToArray();
        var needed = remaining
            .Where(candidate => !candidate.EnabledByDependencyOnly)
            .SelectMany(candidate => extensionManager.GetFeatureDependencies(candidate.Id))
            .Select(candidate => candidate.Id)
            .ToHashSet(StringComparer.Ordinal);

        return remaining
            .Where(candidate => candidate.EnabledByDependencyOnly && !needed.Contains(candidate.Id))
            .Select(candidate => candidate.Id)
            .ToArray();
    }

    internal sealed class FeaturePreviewResponse
    {
        /// <summary>Gets the previewed feature.</summary>
        public string FeatureId { get; init; }

        /// <summary>Gets the previewed action, <c>enable</c> or <c>disable</c>.</summary>
        public string Action { get; init; }

        /// <summary>Gets whether the feature is currently enabled.</summary>
        public bool IsEnabled { get; init; }

        /// <summary>Gets whether the action would apply with the supplied <c>force</c> value.</summary>
        public bool IsAllowed { get; init; }

        /// <summary>Gets whether the action needs <c>force</c>, because other features have to change with this one.</summary>
        public bool RequiresForce { get; init; }

        /// <summary>Gets why the action can't apply whatever the value of <c>force</c>, if so.</summary>
        public string BlockedReason { get; init; }

        /// <summary>Gets the features that would be enabled, dependencies first.</summary>
        public string[] FeaturesToEnable { get; init; } = [];

        /// <summary>Gets the features that would be disabled, dependents first.</summary>
        public string[] FeaturesToDisable { get; init; } = [];

        /// <summary>Gets the features enabled by dependency only that would no longer be needed, and disabled with them.</summary>
        public string[] DependencyOnlyFeaturesToDisable { get; init; } = [];
    }
}
