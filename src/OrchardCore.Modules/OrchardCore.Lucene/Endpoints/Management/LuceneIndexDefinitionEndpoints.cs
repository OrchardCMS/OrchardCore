using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene.Models;
using OrchardCore.Lucene.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.Lucene.Endpoints.Management;

internal static class LuceneIndexDefinitionEndpoints
{
    internal const string CapabilityName = "indexes-lucene";

    internal static void AddLuceneIndexDefinitionEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/indexes/lucene/by-id", GetAsync), "ApiGetLuceneIndexDefinition", "show", "Shows a Lucene content index definition.")
            .Produces<LuceneIndexDefinitionResponse>();
        Configure(routes.MapPost("api/indexes/lucene", CreateAsync), "ApiCreateLuceneIndexDefinition", "create", "Creates a Lucene content index and schedules synchronization.")
            .Produces<LuceneIndexDefinitionResponse>(201).Produces<LuceneIndexDefinitionResponse>().ProducesProblem(409);
        Configure(routes.MapPut("api/indexes/lucene/by-id", UpdateAsync), "ApiUpdateLuceneIndexDefinition", "update", "Updates a definition; reindexing is a separate operation.")
            .Produces<LuceneIndexDefinitionResponse>();
        Configure(routes.MapDelete("api/indexes/lucene/by-id", DeleteAsync), "ApiDeleteLuceneIndexDefinition", "delete", "Removes a Lucene content index and its profile.")
            .Produces(204);
        Configure(routes.MapGet("api/indexes/lucene/analyzers", AnalyzersAsync), "ApiListLuceneIndexAnalyzers", "analyzers", "Lists registered Lucene analyzer names.")
            .Produces<string[]>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder route, string name, string command, string summary)
    {
        var cli = new CliOperationMetadata(["indexes", "lucene"], command)
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
        return IsLuceneContent(profile) ? TypedResults.Ok(Describe(profile)) : context.ApiNotFoundProblem();
    }

    internal static async Task<IResult> AnalyzersAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] LuceneAnalyzerManager analyzers)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        return TypedResults.Ok(analyzers.GetAnalyzers().Select(analyzer => analyzer.Name).Order(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromServices] IIndexProfileManagementService management, [FromBody] LuceneIndexDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (Validate(definition) is { } invalid) { return invalid; }
        var existing = await profiles.FindByNameAsync(definition.Name);
        if (existing is not null)
        {
            return IsLuceneContent(existing) && Equivalent(existing, definition) ? TypedResults.Ok(Describe(existing))
                : TypedResults.Problem("An index with this display name already exists with a different definition.", statusCode: 409);
        }
        if (await profiles.FindByNameAndProviderAsync(definition.IndexName, LuceneConstants.ProviderName) is not null)
        {
            return TypedResults.Problem("An index with this provider index name already exists.", statusCode: 409);
        }
        var profile = await profiles.NewAsync(LuceneConstants.ProviderName, IndexingConstants.ContentsIndexSource, ToData(definition));
        try
        {
            var result = await management.CreateAsync(profile);
            return result == IndexProfileManagementResult.Success
                ? TypedResults.Created($"{context.Request.PathBase}/api/indexes/lucene/by-id?id={Uri.EscapeDataString(profile.Id)}", Describe(profile))
                : MutationFailure(result);
        }
        catch (IndexProfileValidationException exception) { return ValidationFailure(exception.Errors); }
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromQuery] string id, [FromBody] LuceneIndexDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        if (Validate(definition) is { } invalid) { return invalid; }
        var profile = await profiles.FindByIdAsync(id);
        if (!IsLuceneContent(profile)) { return context.ApiNotFoundProblem(); }
        if (!string.Equals(profile.IndexName, definition.IndexName, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The provider index name cannot be changed after creation.", statusCode: 400);
        }
        if (!Equivalent(profile, definition))
        {
            try { await profiles.UpdateAsync(profile, ToData(definition)); }
            catch (IndexProfileValidationException exception) { return ValidationFailure(exception.Errors); }
        }
        return TypedResults.Ok(Describe(profile));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager profiles, [FromServices] IIndexProfileManagementService management, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        var profile = await profiles.FindByIdAsync(id);
        if (profile is null) { return TypedResults.NoContent(); }
        if (!IsLuceneContent(profile)) { return context.ApiNotFoundProblem(); }
        var result = await management.DeleteAsync(profile);
        return result == IndexProfileManagementResult.Success ? TypedResults.NoContent() : MutationFailure(result);
    }

    private static ProblemHttpResult MutationFailure(IndexProfileManagementResult result) => TypedResults.Problem(
        result == IndexProfileManagementResult.LocalDeleteFailed ? "The local index profile could not be removed. Inspect the index before retrying."
            : "The index provider is unavailable or rejected the operation.", statusCode: 503);

    private static bool IsLuceneContent(IndexProfile profile) => profile is not null &&
        string.Equals(profile.ProviderName, LuceneConstants.ProviderName, StringComparison.OrdinalIgnoreCase) &&
        profile.Type == IndexingConstants.ContentsIndexSource;

    private static bool Equivalent(IndexProfile profile, LuceneIndexDefinition definition) =>
        JsonSerializer.Serialize(Describe(profile).Definition) == JsonSerializer.Serialize(definition);

    internal static JsonObject ToData(LuceneIndexDefinition definition) => JsonSerializer.SerializeToNode(definition).AsObject();

    internal static LuceneIndexDefinitionResponse Describe(IndexProfile profile)
    {
        var content = profile.TryGet<ContentIndexMetadata>(out var contentMetadata) ? contentMetadata : new ContentIndexMetadata();
        var lucene = profile.TryGet<LuceneIndexMetadata>(out var luceneMetadata) ? luceneMetadata : new LuceneIndexMetadata();
        var query = profile.TryGet<LuceneIndexDefaultQueryMetadata>(out var queryMetadata) ? queryMetadata : new LuceneIndexDefaultQueryMetadata();
        return new LuceneIndexDefinitionResponse
        {
            Id = profile.Id,
            Definition = new LuceneIndexDefinition
            {
                Name = profile.Name, IndexName = profile.IndexName, IndexLatest = content.IndexLatest,
                IndexedContentTypes = content.IndexedContentTypes ?? [], Culture = string.IsNullOrEmpty(content.Culture) ? "any" : content.Culture,
                AnalyzerName = lucene.AnalyzerName ?? LuceneConstants.DefaultAnalyzer, StoreSourceData = lucene.StoreSourceData,
                QueryAnalyzerName = query.QueryAnalyzerName ?? LuceneConstants.DefaultAnalyzer, AllowLuceneQueries = query.AllowLuceneQueries,
                DefaultVersion = query.DefaultVersion,
                DefaultSearchFields = query.DefaultSearchFields ?? [ContentIndexingConstants.FullTextKey],
            },
        };
    }

    private static IResult Validate(LuceneIndexDefinition definition)
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
        return errors.Count == 0 ? null : ValidationFailure(errors);
    }

    private static ValidationProblem ValidationFailure(IEnumerable<ValidationResult> errors) => TypedResults.ValidationProblem(errors
        .SelectMany(error => error.MemberNames.DefaultIfEmpty(string.Empty).Select(member => (member, error.ErrorMessage)))
        .GroupBy(error => error.member).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()));

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, IndexingPermissions.ManageIndexes);
}
