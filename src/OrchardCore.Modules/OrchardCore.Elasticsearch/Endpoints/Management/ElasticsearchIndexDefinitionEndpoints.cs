using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Contents.Indexing;
using OrchardCore.Elasticsearch.Core.Models;
using OrchardCore.Elasticsearch.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.Elasticsearch.Endpoints.Management;

internal static class ElasticsearchIndexDefinitionEndpoints
{
    internal const string CapabilityName = "indexes-elasticsearch";

    internal static void AddElasticsearchIndexDefinitionEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/indexes/elasticsearch/by-id", GetAsync), "ApiGetElasticsearchIndexDefinition", "show", "Shows an Elasticsearch content index definition.")
            .Produces<ElasticsearchIndexDefinitionResponse>().ProducesETag();
        Configure(routes.MapPost("api/indexes/elasticsearch", CreateAsync), "ApiCreateElasticsearchIndexDefinition", "create", "Creates an Elasticsearch content index and schedules synchronization.")
            .Produces<ElasticsearchIndexDefinitionResponse>(201).Produces<ElasticsearchIndexDefinitionResponse>().ProducesETag().ProducesProblem(409);
        Configure(routes.MapPut("api/indexes/elasticsearch/by-id", UpdateAsync), "ApiUpdateElasticsearchIndexDefinition", "update", "Updates a definition; reindexing is a separate operation.")
            .Produces<ElasticsearchIndexDefinitionResponse>().AcceptsIfMatch();
        Configure(routes.MapDelete("api/indexes/elasticsearch/by-id", DeleteAsync), "ApiDeleteElasticsearchIndexDefinition", "delete", "Removes an Elasticsearch content index and its profile.")
            .Produces(204);
        Configure(routes.MapGet("api/indexes/elasticsearch/analyzers", AnalyzersAsync), "ApiListElasticsearchIndexAnalyzers", "analyzers", "Lists configured Elasticsearch analyzer names.")
            .Produces<string[]>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder route, string name, string command, string summary)
    {
        var cli = new CliOperationMetadata(["indexes", "elasticsearch"], command)
        {
            Capability = CapabilityName,
            InputMode = command is "create" or "update" ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = command == "delete",
        };
        if (command is "show" or "update" or "delete") { cli.Arguments.Add(new CliArgumentMetadata("id", 0)); }
        return route.WithName(name).WithTags("Indexes").WithSummary(summary).WithCliCommand(cli).DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement), new PermissionRequirement(IndexingPermissions.ManageIndexes)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(503);
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        var profile = await profiles.FindByIdAsync(id);
        return IsContentIndex(profile) ? TypedResults.Ok(context.WithApiETag(Describe(profile))) : context.ApiNotFoundProblem();
    }

    internal static async Task<IResult> AnalyzersAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOptions<ElasticsearchOptions> options)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        return TypedResults.Ok(GetAnalyzers(options.Value));
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromServices] IIndexProfileManagementService management,
        [FromServices] IOptions<ElasticsearchOptions> options, [FromBody] ElasticsearchIndexDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (Validate(definition, options.Value) is { } invalid) { return invalid; }
        var existing = await profiles.FindByNameAsync(definition.Name);
        if (existing is not null)
        {
            return IsContentIndex(existing) && Equivalent(existing, definition) ? TypedResults.Ok(context.WithApiETag(Describe(existing)))
                : TypedResults.Problem("An index with this display name already exists with a different definition.", statusCode: 409);
        }
        if (await profiles.FindByNameAndProviderAsync(definition.IndexName, ElasticsearchConstants.ProviderName) is not null)
        {
            return TypedResults.Problem("An index with this provider index name already exists.", statusCode: 409);
        }
        var profile = await profiles.NewAsync(ElasticsearchConstants.ProviderName, IndexingConstants.ContentsIndexSource, ToData(definition));
        try
        {
            var result = await management.CreateAsync(profile);
            return result == IndexProfileManagementResult.Success
                ? TypedResults.Created($"{context.Request.PathBase}/api/indexes/elasticsearch/by-id?id={Uri.EscapeDataString(profile.Id)}", context.WithApiETag(Describe(profile)))
                : MutationFailure(result);
        }
        catch (IndexProfileValidationException exception) { return ValidationFailure(exception.Errors); }
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromServices] IOptions<ElasticsearchOptions> options, [FromQuery] string id, [FromBody] ElasticsearchIndexDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        if (Validate(definition, options.Value) is { } invalid) { return invalid; }
        var profile = await profiles.FindByIdAsync(id);
        if (!IsContentIndex(profile)) { return context.ApiNotFoundProblem(); }
        if (context.ApiIfMatchProblem(Describe(profile)) is { } preconditionFailed) { return preconditionFailed; }
        if (!string.Equals(profile.IndexName, definition.IndexName, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The provider index name cannot be changed after creation.", statusCode: 400);
        }
        if (!Equivalent(profile, definition))
        {
            ApplyProviderSettings(profile, definition);
            try { await profiles.UpdateAsync(profile, ToData(definition)); }
            catch (IndexProfileValidationException exception) { return ValidationFailure(exception.Errors); }
        }
        return TypedResults.Ok(context.WithApiETag(Describe(profile)));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromServices] IIndexProfileManagementService management, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        var profile = await profiles.FindByIdAsync(id);
        if (profile is null || !IsContentIndex(profile)) { return context.ApiNotFoundProblem(); }
        var result = await management.DeleteAsync(profile);
        return result == IndexProfileManagementResult.Success ? TypedResults.NoContent() : MutationFailure(result);
    }

    private static ProblemHttpResult MutationFailure(IndexProfileManagementResult result) => TypedResults.Problem(
        result == IndexProfileManagementResult.LocalDeleteFailed ? "The local index profile could not be removed. Inspect the index before retrying."
            : "The index provider is unavailable or rejected the operation.", statusCode: 503);

    private static bool IsContentIndex(IndexProfile profile) => profile is not null &&
        string.Equals(profile.ProviderName, ElasticsearchConstants.ProviderName, StringComparison.OrdinalIgnoreCase) &&
        profile.Type == IndexingConstants.ContentsIndexSource;

    private static bool Equivalent(IndexProfile profile, ElasticsearchIndexDefinition definition) =>
        JsonSerializer.Serialize(Describe(profile).Definition) == JsonSerializer.Serialize(definition);

    internal static JsonObject ToData(ElasticsearchIndexDefinition definition) => JsonSerializer.SerializeToNode(definition).AsObject();

    // The Elasticsearch profile handler only reads these settings from the data of a new profile.
    internal static void ApplyProviderSettings(IndexProfile profile, ElasticsearchIndexDefinition definition)
    {
        var metadata = profile.GetOrCreate<ElasticsearchIndexMetadata>();
        metadata.AnalyzerName = definition.AnalyzerName;
        metadata.StoreSourceData = definition.StoreSourceData;
        profile.Put(metadata);

        var query = profile.GetOrCreate<ElasticsearchDefaultQueryMetadata>();
        query.QueryAnalyzerName = definition.QueryAnalyzerName;
        query.DefaultSearchFields = definition.DefaultSearchFields;
        profile.Put(query);
    }

    internal static ElasticsearchIndexDefinitionResponse Describe(IndexProfile profile)
    {
        var content = profile.TryGet<ContentIndexMetadata>(out var contentMetadata) ? contentMetadata : new ContentIndexMetadata();
        var provider = profile.TryGet<ElasticsearchIndexMetadata>(out var providerMetadata) ? providerMetadata : new ElasticsearchIndexMetadata();
        var query = profile.TryGet<ElasticsearchDefaultQueryMetadata>(out var queryMetadata) ? queryMetadata : new ElasticsearchDefaultQueryMetadata();
        return new ElasticsearchIndexDefinitionResponse
        {
            Id = profile.Id,
            Definition = new ElasticsearchIndexDefinition
            {
                Name = profile.Name, IndexName = profile.IndexName, IndexLatest = content.IndexLatest,
                IndexedContentTypes = content.IndexedContentTypes ?? [], Culture = string.IsNullOrEmpty(content.Culture) ? "any" : content.Culture,
                AnalyzerName = provider.AnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer, StoreSourceData = provider.StoreSourceData,
                QueryAnalyzerName = query.QueryAnalyzerName ?? ElasticsearchConstants.DefaultAnalyzer,
                DefaultSearchFields = query.DefaultSearchFields ?? [ContentIndexingConstants.FullTextKey],
            },
        };
    }

    internal static string[] GetAnalyzers(ElasticsearchOptions options) =>
        options.Analyzers.Keys.Append(ElasticsearchConstants.DefaultAnalyzer).Distinct(StringComparer.Ordinal).Order(StringComparer.OrdinalIgnoreCase).ToArray();

    internal static IResult Validate(ElasticsearchIndexDefinition definition, ElasticsearchOptions options)
    {
        if (definition is null) { return TypedResults.Problem("A definition is required.", statusCode: 400); }
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(definition, new ValidationContext(definition), errors, validateAllProperties: true);
        if (string.IsNullOrWhiteSpace(definition.Culture) || string.IsNullOrWhiteSpace(definition.AnalyzerName) ||
            string.IsNullOrWhiteSpace(definition.QueryAnalyzerName) || definition.DefaultSearchFields is null)
        {
            errors.Add(new ValidationResult("Culture, analyzer names, and default search fields cannot be null or blank."));
        }
        if (definition.IndexedContentTypes?.Any(string.IsNullOrWhiteSpace) == true || definition.DefaultSearchFields?.Any(string.IsNullOrWhiteSpace) == true)
        {
            errors.Add(new ValidationResult("Content type and search field names cannot be blank."));
        }
        if (definition.Name != definition.Name?.Trim() || definition.Culture != definition.Culture?.Trim() ||
            definition.IndexedContentTypes?.Distinct(StringComparer.Ordinal).Count() != definition.IndexedContentTypes?.Length)
        {
            errors.Add(new ValidationResult("Names and culture must not have surrounding whitespace, and content types must be unique."));
        }
        var analyzers = GetAnalyzers(options);
        foreach (var (member, value) in new[] { (nameof(definition.AnalyzerName), definition.AnalyzerName), (nameof(definition.QueryAnalyzerName), definition.QueryAnalyzerName) })
        {
            if (!string.IsNullOrWhiteSpace(value) && !analyzers.Contains(value, StringComparer.Ordinal))
            {
                errors.Add(new ValidationResult($"The analyzer '{value}' is not configured.", [member]));
            }
        }
        return errors.Count == 0 ? null : ValidationFailure(errors);
    }

    private static ValidationProblem ValidationFailure(IEnumerable<ValidationResult> errors) => TypedResults.ValidationProblem(errors
        .SelectMany(error => error.MemberNames.DefaultIfEmpty(string.Empty).Select(member => (member, error.ErrorMessage)))
        .GroupBy(error => error.member).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, IndexingPermissions.ManageIndexes);
}
