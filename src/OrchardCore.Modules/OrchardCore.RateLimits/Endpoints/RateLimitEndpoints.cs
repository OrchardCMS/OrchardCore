using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;
using OrchardCore.RateLimits.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.RateLimits.Endpoints;

internal static class RateLimitEndpoints
{
    private const string Prefix = "api/rate-limits/policies";

    public static void AddRateLimitEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet(Prefix, ListAsync), "ApiListRateLimitPolicies", "list", "Lists tenant rate-limit policies.")
            .Produces<IReadOnlyList<RateLimitPolicyResponse>>();
        Configure(routes.MapGet(Prefix + "/by-id", GetAsync), "ApiGetRateLimitPolicy", "show", "Shows a rate-limit policy.", ["policyId"])
            .Produces<RateLimitPolicyResponse>();
        Configure(routes.MapPost(Prefix, CreateAsync), "ApiCreateRateLimitPolicy", "create", "Creates a disabled policy; identical name retries return the existing policy.", input: true)
            .Produces<RateLimitPolicyResponse>(201);
        Configure(routes.MapPut(Prefix + "/by-id", UpdateAsync), "ApiUpdateRateLimitPolicy", "update", "Replaces policy metadata and target; disable first to change its target.", ["policyId"], input: true)
            .Produces<RateLimitPolicyResponse>();
        Configure(routes.MapDelete(Prefix + "/by-id", DeleteAsync), "ApiDeleteRateLimitPolicy", "delete", "Deletes a policy; an absent policy is unchanged.", ["policyId"])
            .Produces(204);
        Configure(routes.MapPost(Prefix + "/enable", EnableAsync), "ApiEnableRateLimitPolicy", "enable", "Enables a policy and reloads tenant rate limiting.", ["policyId"])
            .Produces(204);
        Configure(routes.MapPost(Prefix + "/disable", DisableAsync), "ApiDisableRateLimitPolicy", "disable", "Disables a policy and reloads tenant rate limiting.", ["policyId"])
            .Produces(204);
        Configure(routes.MapGet("api/rate-limits/limiter-types", SourcesAsync), "ApiListRateLimitSources", "list", "Discovers built-in limiter configuration schemas.", group: ["rate-limits", "limiter-types"])
            .Produces<IReadOnlyList<RateLimitSourceResponse>>();
        Configure(routes.MapGet(Prefix + "/limiters", ListLimitersAsync), "ApiListRateLimitLimiters", "list", "Lists a policy's limiters.", ["policyId"], group: ["rate-limits", "policies", "limiters"])
            .Produces<IReadOnlyList<RateLimitLimiterResponse>>();
        Configure(routes.MapGet(Prefix + "/limiters/by-id", GetLimiterAsync), "ApiGetRateLimitLimiter", "show", "Shows a policy limiter's supported settings.", ["policyId", "limiterId"], group: ["rate-limits", "policies", "limiters"])
            .Produces<RateLimitLimiterResponse>();
        Configure(routes.MapPost(Prefix + "/limiters", AddLimiterAsync), "ApiAddRateLimitLimiter", "add", "Adds a limiter to a disabled policy using a stable caller-selected ID.", ["policyId"], true, ["rate-limits", "policies", "limiters"])
            .Produces<RateLimitLimiterResponse>();
        Configure(routes.MapPut(Prefix + "/limiters/by-id", UpdateLimiterAsync), "ApiUpdateRateLimitLimiter", "update", "Replaces a disabled policy limiter's complete settings.", ["policyId", "limiterId"], true, ["rate-limits", "policies", "limiters"])
            .Produces<RateLimitLimiterResponse>();
        Configure(routes.MapDelete(Prefix + "/limiters/by-id", DeleteLimiterAsync), "ApiDeleteRateLimitLimiter", "delete", "Deletes a limiter from a disabled policy; an absent limiter is unchanged.", ["policyId", "limiterId"], group: ["rate-limits", "policies", "limiters"])
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary,
        string[] arguments = null, bool input = false, string[] group = null)
    {
        var metadata = new CliOperationMetadata(group ?? ["rate-limits", "policies"], verb)
        {
            Capability = "rate-limits",
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = verb == "delete",
        };
        for (var index = 0; index < (arguments?.Length ?? 0); index++)
        {
            metadata.Arguments.Add(new CliArgumentMetadata(arguments[index], index));
        }
        return builder.WithName(name).WithTags("Rate Limits").WithSummary(summary).WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(
                    new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(RateLimitsPermissions.ManageRateLimits)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(409);
    }

    private static Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, RateLimitsPermissions.ManageRateLimits);

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromQuery] int? skip, [FromQuery] int? take)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (skip < 0 || take < 1 || take > 200) { return Invalid("Skip must be nonnegative and take must be between 1 and 200."); }
        var all = await policies.GetAllAsync(PolicyVersion.Current);
        return TypedResults.Ok<IReadOnlyList<RateLimitPolicyResponse>>(all.OrderBy(policy => policy.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(policy => policy.PolicyId, StringComparer.Ordinal).Skip(skip ?? 0).Take(take ?? 50).Select(Describe).ToArray());
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromQuery] string policyId)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        return policy is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(policy));
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitPolicyMutations mutations, [FromBody] RateLimitPolicyInput input)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var error = Validate(input, out var scope);
        if (error is not null) { return Invalid(error); }
        var all = await policies.GetAllAsync(PolicyVersion.Current);
        var existing = all.FirstOrDefault(policy => string.Equals(policy.Name, input.Name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return Equal(Definition(existing), Normalize(input))
                ? TypedResults.Created(Location(context, existing.PolicyId), Describe(existing))
                : Conflict("A policy with this name already has a different definition.");
        }
        var policy = new RateLimitPolicy
        {
            Name = input.Name.Trim(), Description = input.Description?.Trim(), Scope = scope,
            Path = input.Path, GroupName = input.GroupName,
            OwnerId = context.User.FindFirstValue(ClaimTypes.NameIdentifier), Author = context.User.Identity?.Name,
        };
        await mutations.CreateAsync(policy);
        return TypedResults.Created(Location(context, policy.PolicyId), Describe(policy));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitPolicyMutations mutations,
        [FromQuery] string policyId, [FromBody] RateLimitPolicyInput input)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var error = Validate(input, out var scope);
        if (error is not null) { return Invalid(error); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        if (policy is null) { return context.ApiNotFoundProblem(); }
        if (policy.IsEnabled && (scope != policy.Scope || input.Path != policy.Path || input.GroupName != policy.GroupName))
        {
            return Conflict("Disable the policy before changing its target.");
        }
        var all = await policies.GetAllAsync(PolicyVersion.Current);
        if (all.Any(other => other.PolicyId != policyId && string.Equals(other.Name, input.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict("Another policy already uses this name.");
        }
        if (!Equal(Definition(policy), Normalize(input)))
        {
            policy.Name = input.Name.Trim();
            policy.Description = input.Description?.Trim();
            policy.Scope = scope;
            policy.Path = input.Path;
            policy.GroupName = input.GroupName;
            await mutations.UpdateAsync(policy, policy.IsEnabled);
        }
        return TypedResults.Ok(Describe(policy));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitPolicyMutations mutations, [FromQuery] string policyId)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        if (policy is not null) { await mutations.DeleteAsync([policy]); }
        return TypedResults.NoContent();
    }

    internal static Task<IResult> EnableAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitPolicyMutations mutations, [FromQuery] string policyId) =>
        StatusAsync(context, authorization, policies, mutations, policyId, true);

    internal static Task<IResult> DisableAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitPolicyMutations mutations, [FromQuery] string policyId) =>
        StatusAsync(context, authorization, policies, mutations, policyId, false);

    private static async Task<IResult> StatusAsync(HttpContext context, IAuthorizationService authorization,
        IRateLimitPolicyStore policies, RateLimitPolicyMutations mutations, string policyId, bool enabled)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        if (policy is null) { return context.ApiNotFoundProblem(); }
        await mutations.SetStatusAsync([policy], enabled);
        return TypedResults.NoContent();
    }

    internal static async Task<IResult> ListLimitersAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromQuery] string policyId)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        return policy is null ? context.ApiNotFoundProblem() : TypedResults.Ok<IReadOnlyList<RateLimitLimiterResponse>>(policy.Limiters.Select(Describe).ToArray());
    }

    internal static async Task<IResult> GetLimiterAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromQuery] string policyId, [FromQuery] string limiterId)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        var limiter = policy?.Limiters.FirstOrDefault(item => item.Id == limiterId);
        return limiter is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(limiter));
    }

    internal static Task<IResult> AddLimiterAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitLimiterMutations mutations,
        [FromServices] IStringLocalizer<RateLimitLimiterInput> localizer, [FromQuery] string policyId, [FromBody] RateLimitLimiterInput input) =>
        SaveLimiterAsync(context, authorization, policies, mutations, localizer, policyId, null, input);

    internal static Task<IResult> UpdateLimiterAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitLimiterMutations mutations,
        [FromServices] IStringLocalizer<RateLimitLimiterInput> localizer, [FromQuery] string policyId, [FromQuery] string limiterId,
        [FromBody] RateLimitLimiterInput input) => SaveLimiterAsync(context, authorization, policies, mutations, localizer, policyId, limiterId, input);

    private static async Task<IResult> SaveLimiterAsync(HttpContext context, IAuthorizationService authorization,
        IRateLimitPolicyStore policies, RateLimitLimiterMutations mutations, IStringLocalizer localizer,
        string policyId, string limiterId, RateLimitLimiterInput input)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(input?.Id) || input.Id.Length > 128 || !RateLimitLimiterConfiguration.IsSupported(input.Source)
            || (limiterId is not null && limiterId != input.Id)) { return Invalid("A matching limiter ID and a supported source are required."); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        if (policy is null) { return context.ApiNotFoundProblem(); }
        var existing = policy.Limiters.FirstOrDefault(item => item.Id == input.Id);
        if (limiterId is not null && existing is null) { return context.ApiNotFoundProblem(); }
        if (existing is not null && existing.Source != input.Source) { return Conflict("The limiter source cannot change."); }
        var candidate = new RateLimitLimiter
        {
            Id = input.Id, Source = input.Source,
            Properties = existing?.Properties.DeepClone().AsObject() ?? new JsonObject(),
        };
        var errors = RateLimitLimiterConfiguration.Configure(candidate, input.Values, localizer);
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors); }
        if (existing is not null && JsonNode.DeepEquals(RateLimitLimiterConfiguration.Describe(existing), RateLimitLimiterConfiguration.Describe(candidate)))
        {
            return TypedResults.Ok(Describe(existing));
        }
        if (!RateLimitLimiterMutations.CanModify(policy)) { return Conflict("Disable the policy before changing its limiters."); }
        if (existing is not null && limiterId is null) { return Conflict("The limiter ID already has different settings."); }
        await mutations.SaveAsync(policy, candidate);
        return TypedResults.Ok(Describe(candidate));
    }

    internal static async Task<IResult> DeleteLimiterAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IRateLimitPolicyStore policies, [FromServices] RateLimitLimiterMutations mutations,
        [FromQuery] string policyId, [FromQuery] string limiterId)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(policyId)) { return Invalid("A policy ID is required."); }
        var policy = await policies.FindByIdAsync(policyId, PolicyVersion.Current);
        if (policy is null) { return context.ApiNotFoundProblem(); }
        if (!policy.Limiters.Any(item => item.Id == limiterId)) { return TypedResults.NoContent(); }
        if (!RateLimitLimiterMutations.CanModify(policy)) { return Conflict("Disable the policy before changing its limiters."); }
        await mutations.DeleteAsync(policy, limiterId);
        return TypedResults.NoContent();
    }

    internal static async Task<IResult> SourcesAsync(HttpContext context, [FromServices] IAuthorizationService authorization)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        return TypedResults.Ok<IReadOnlyList<RateLimitSourceResponse>>(RateLimitLimiterConfiguration.Sources.Select(source =>
            new RateLimitSourceResponse { Source = source, Schema = Schema(source) }).ToArray());
    }

    private static JsonObject Schema(string source)
    {
        var defaults = RateLimitLimiterConfiguration.Describe(new RateLimitLimiter { Source = source });
        var properties = new JsonObject();
        var required = new JsonArray();
        foreach (var property in defaults)
        {
            properties[property.Key] = property.Key == "queueProcessingOrder"
                ? new JsonObject { ["type"] = "string", ["enum"] = new JsonArray("OldestFirst", "NewestFirst") }
                : new JsonObject { ["type"] = "integer", ["minimum"] = property.Key == "queueLimit" ? 0 : 1, ["maximum"] = int.MaxValue };
            if (property.Key is not "queueLimit" and not "queueProcessingOrder") { required.Add(property.Key); }
        }
        return new JsonObject { ["type"] = "object", ["additionalProperties"] = false, ["required"] = required, ["properties"] = properties };
    }

    private static string Validate(RateLimitPolicyInput input, out RateLimitPolicyScope scope)
    {
        scope = default;
        if (string.IsNullOrWhiteSpace(input?.Name) || input.Name.Trim().Length > 256) { return "A policy name of at most 256 characters is required."; }
        if (input.Scope is not ("Global" or "Endpoint" or "Group") || !Enum.TryParse(input.Scope, false, out scope)) { return "Scope must be Global, Endpoint, or Group."; }
        return RateLimitPolicyValidation.ValidateTarget(scope, input.Path, input.GroupName) switch
        {
            RateLimitPolicyTargetError.None => null,
            RateLimitPolicyTargetError.MissingPath or RateLimitPolicyTargetError.RelativePath => "Endpoint scope requires an absolute path prefix.",
            RateLimitPolicyTargetError.MissingGroup => "Group scope requires a group name.",
            _ => "Select a supported policy scope.",
        };
    }

    private static RateLimitPolicyInput Normalize(RateLimitPolicyInput input) => new()
    {
        Name = input.Name.Trim(), Description = input.Description?.Trim(), Scope = input.Scope, Path = input.Path, GroupName = input.GroupName,
    };
    private static bool Equal(RateLimitPolicyInput left, RateLimitPolicyInput right) =>
        string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase) && left.Description == right.Description
        && left.Scope == right.Scope && left.Path == right.Path && left.GroupName == right.GroupName;
    private static RateLimitPolicyInput Definition(RateLimitPolicy policy) => new()
    {
        Name = policy.Name, Description = policy.Description, Scope = policy.Scope.ToString(), Path = policy.Path, GroupName = policy.GroupName,
    };
    private static RateLimitPolicyResponse Describe(RateLimitPolicy policy) => new()
    {
        PolicyId = policy.PolicyId, Definition = Definition(policy), IsEnabled = policy.IsEnabled, EnabledUtc = policy.EnabledUtc,
        Limiters = policy.Limiters.Select(Describe).ToArray(),
    };
    private static RateLimitLimiterResponse Describe(RateLimitLimiter limiter) => new()
    {
        Id = limiter.Id, Source = limiter.Source, CanConfigure = RateLimitLimiterConfiguration.IsSupported(limiter.Source),
        Values = RateLimitLimiterConfiguration.Describe(limiter),
    };
    private static ProblemHttpResult Invalid(string message) => TypedResults.Problem(message, statusCode: 400);
    private static ProblemHttpResult Conflict(string message) => TypedResults.Problem(message, statusCode: 409);
    private static string Location(HttpContext context, string id) => $"{context.Request.PathBase}/{Prefix}/by-id?policyId={Uri.EscapeDataString(id)}";
}
