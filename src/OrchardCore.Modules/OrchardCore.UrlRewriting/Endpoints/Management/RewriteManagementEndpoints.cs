using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Endpoints.Management;

internal static class RewriteManagementEndpoints
{
    private const string Prefix = "api/url-rewriting/rules";

    internal static void MapManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet(Prefix, ListAsync), "ApiListUrlRewriteRules", "list", "Lists rules in runtime order.").Produces<RewritePage>();
        Configure(routes.MapGet(Prefix + "/{id}", GetAsync), "ApiGetUrlRewriteRule", "show", "Shows a stored rule.", argument: true).Produces<RewriteResponse>().ProducesProblem(404);
        Configure(routes.MapGet("api/url-rewriting/sources", SourcesAsync), "ApiListUrlRewriteSources", "sources", "Lists registered sources and remote editing support.").Produces<RewriteSourceResponse[]>();
        Configure(routes.MapPost(Prefix + "/validate", ValidateAsync), "ApiValidateUrlRewriteRule", "validate", "Validates a complete definition without saving or matching a request.", input: true).Produces<RewriteValidationResponse>();
        Configure(routes.MapPost(Prefix, CreateAsync), "ApiCreateUrlRewriteRule", "create", "Creates a rule; an identical retry with the same explicit id returns the stored rule.", input: true).Produces<RewriteResponse>(201).Produces<RewriteResponse>().ProducesProblem(409);
        Configure(routes.MapPut(Prefix + "/{id}", UpdateAsync), "ApiUpdateUrlRewriteRule", "update", "Replaces the complete built-in rule definition without changing its source or order.", argument: true, input: true).Produces<RewriteResponse>().ProducesProblem(404);
        Configure(routes.MapDelete(Prefix + "/{id}", DeleteAsync), "ApiDeleteUrlRewriteRule", "delete", "Deletes a rule; a missing rule is a no-op.", argument: true, confirmation: true).Produces(204);
        Configure(routes.MapPut(Prefix + "/{id}/position", MoveAsync), "ApiMoveUrlRewriteRule", "move", "Moves a rule to a zero-based position in the complete ordered list.", argument: true, input: true).Produces<RewriteResponse>().ProducesProblem(404);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string verb, string summary,
        bool argument = false, bool input = false, bool confirmation = false)
    {
        var metadata = new CliOperationMetadata(["url-rewriting", "rules"], verb)
        {
            Capability = "url-rewriting", InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = confirmation,
        };
        if (argument)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("id", 0));
        }
        return builder.WithName(operationId).WithTags("URL Rewriting").WithSummary(summary).WithCliCommand(metadata)
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(UrlRewritingPermissions.ManageUrlRewritingRules)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, [FromQuery] int skip = 0, [FromQuery] int take = 50, [FromQuery] string search = null)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        if (skip < 0 || take is < 1 or > 200)
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var rules = (await manager.GetAllAsync()).Where(rule => string.IsNullOrEmpty(search)
            || (rule.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToArray();
        return TypedResults.Ok(new RewritePage { Skip = skip, Take = take, TotalCount = rules.Length,
            Items = rules.Skip(skip).Take(take).Select(RewriteDefinitionMapper.Describe).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, string id)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var rule = await manager.FindByIdAsync(id);
        return rule is null ? context.ApiNotFoundProblem() : TypedResults.Ok(RewriteDefinitionMapper.Describe(rule));
    }

    internal static async Task<IResult> SourcesAsync(HttpContext context, IAuthorizationService authorization,
        [FromServices] IEnumerable<IUrlRewriteRuleSource> sources) => !await AuthorizedAsync(context, authorization)
        ? context.ApiForbidProblem() : TypedResults.Ok(sources.Select(source => new RewriteSourceResponse
        {
            Name = source.TechnicalName, DisplayName = source.DisplayName.Value, Description = source.Description.Value,
            IsWritable = RewriteDefinitionMapper.Supported(source.TechnicalName),
        }).OrderBy(source => source.Name, StringComparer.Ordinal).ToArray());

    internal static async Task<IResult> ValidateAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, [FromBody] RewriteDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var (rule, errors) = await PrepareAsync(manager, definition);
        return TypedResults.Ok(new RewriteValidationResponse { IsValid = rule is not null && errors.Count == 0, Errors = errors });
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, [FromBody] RewriteDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var (rule, errors) = await PrepareAsync(manager, definition);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }
        var existing = await manager.FindByIdAsync(rule.Id);
        if (existing is not null)
        {
            return Equivalent(existing, rule) ? TypedResults.Ok(RewriteDefinitionMapper.Describe(existing))
                : TypedResults.Problem("The identifier already belongs to a different rule definition.", statusCode: 409);
        }
        await manager.SaveAsync(rule);
        return TypedResults.Created($"{context.Request.PathBase}/{Prefix}/{Uri.EscapeDataString(rule.Id)}",
            RewriteDefinitionMapper.Describe(rule));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, string id, [FromBody] RewriteDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var existing = await manager.FindByIdAsync(id);
        if (existing is null)
        {
            return context.ApiNotFoundProblem();
        }
        var (rule, errors) = await PrepareAsync(manager, definition, existing);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }
        await manager.SaveAsync(rule);
        return TypedResults.Ok(RewriteDefinitionMapper.Describe(rule));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, string id)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var rule = await manager.FindByIdAsync(id);
        if (rule is not null)
        {
            await manager.DeleteAsync(rule);
        }
        return TypedResults.NoContent();
    }

    internal static async Task<IResult> MoveAsync(HttpContext context, IAuthorizationService authorization,
        IRewriteRulesManager manager, string id, [FromBody] RewriteMoveRequest request)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var rules = (await manager.GetAllAsync()).ToArray();
        var index = Array.FindIndex(rules, rule => rule.Id == id);
        if (index < 0)
        {
            return context.ApiNotFoundProblem();
        }
        if (request?.Position is not int position || position < 0 || position >= rules.Length)
        {
            return TypedResults.Problem("Position must be a zero-based index in the complete rule list.", statusCode: 400);
        }
        // The existing manager/admin contract is one-based; the API exposes stored zero-based positions.
        await manager.ResortOrderAsync(index + 1, position + 1);
        var moved = rules[index].Clone();
        moved.Order = position;
        return TypedResults.Ok(RewriteDefinitionMapper.Describe(moved));
    }

    private static async Task<(RewriteRule Rule, Dictionary<string, string[]> Errors)> PrepareAsync(
        IRewriteRulesManager manager, RewriteDefinition definition, RewriteRule existing = null)
    {
        if (definition is null || !RewriteDefinitionMapper.Supported(definition.Source))
        {
            return (null, new() { ["source"] = ["A complete definition with the built-in Rewrite or Redirect source is required."] });
        }
        if (definition.Id is not null && !RewriteDefinitionMapper.ValidId(definition.Id))
        {
            return (null, new() { ["id"] = ["Use 1–128 ASCII letters, digits, underscores or hyphens."] });
        }
        var rule = existing?.Clone() ?? await manager.NewAsync(definition.Source);
        if (rule is null)
        {
            return (null, new() { ["source"] = ["The requested source is unavailable."] });
        }
        if (existing is null && definition.Id is not null)
        {
            rule.Id = definition.Id;
        }
        var errors = RewriteDefinitionMapper.Apply(rule, definition);
        var result = await manager.ValidateAsync(rule);
        foreach (var error in result.Errors)
        {
            foreach (var member in error.MemberNames.DefaultIfEmpty("body"))
            {
                var key = char.ToLowerInvariant(member[0]) + member[1..];
                errors[key] = errors.TryGetValue(key, out var messages) ? [.. messages, error.ErrorMessage] : [error.ErrorMessage];
            }
        }
        if (!result.Succeeded && errors.Count == 0)
        {
            errors["body"] = ["The rule was rejected by its validation handlers."];
        }
        return (rule, errors);
    }

    private static bool Equivalent(RewriteRule first, RewriteRule second) => first.Name == second.Name
        && first.Source == second.Source && JsonNode.DeepEquals(first.Properties, second.Properties);

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization)
        => await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement)
        && await authorization.AuthorizeAsync(context.User, UrlRewritingPermissions.ManageUrlRewritingRules);
}
