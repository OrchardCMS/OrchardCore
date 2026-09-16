using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;
using OrchardCore.RemoteManagement;

namespace OrchardCore.DataLocalization.Endpoints;

internal static class TranslationManagementEndpoints
{
    public static void AddTranslationManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/localization/translations").WithTags("Data Localization")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api })
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .DisableAntiforgery();

        group.MapGet("", ListAsync)
            .WithName("ApiListDataTranslations").WithSummary("Lists dynamic translation keys and values.")
            .WithDescription("Lists registered data localization descriptors with their stored translation in a supported culture. Does not read PO catalogs. Requires ViewDynamicTranslations.")
            .Produces<TranslationListResponse>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
        group.MapPut("", SetAsync)
            .WithName("ApiSetDataTranslation").WithSummary("Sets one dynamic translation.")
            .WithDescription("Sets a registered context/key in one supported culture, preserving other entries and cultures. Requires ManageTranslations or the culture-specific ManageTranslations permission.")
            .Accepts<TranslationRequest>("application/json")
            .Produces<TranslationResult>().ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
        group.MapDelete("", DeleteAsync)
            .WithName("ApiDeleteDataTranslation").WithSummary("Removes one stored dynamic translation.")
            .WithDescription("Removes an exact context/key in one supported culture, restoring source text or fallback. Repeating deletion is safe. Requires ManageTranslations or the culture-specific ManageTranslations permission.")
            .Produces<TranslationResult>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILocalizationService localization, [FromServices] ITranslationsManager manager,
        [FromServices] IEnumerable<ILocalizationDataProvider> providers, [AsParameters] TranslationListRequest request)
    {
        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(TranslationManagementEndpoints));

        if (!await authorization.AuthorizeAsync(context.User, DataLocalizationPermissions.ViewDynamicTranslations))
        {
            return context.ApiForbidProblem();
        }

        var culture = await ResolveCultureAsync(localization, request.Culture);
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (culture is null || skip < 0 || take < 1 || take > 200 || request.Search?.Length > 1000)
        {
            return context.ApiBadRequestProblem(detail: S["Provide a supported non-invariant culture, nonnegative skip, take between 1 and 200, and search of at most 1000 characters."]);
        }

        var document = await manager.GetTranslationsDocumentAsync();
        var translations = document.Translations.GetValueOrDefault(culture) ?? [];
        var stored = translations.GroupBy(item => (item.Context, item.Key)).ToDictionary(items => items.Key, items => items.Last().Value);
        var descriptors = await GetDescriptorsAsync(providers);
        var items = descriptors
            .Where(item => string.IsNullOrEmpty(request.Search)
                || item.Context.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || item.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                || (item.Value?.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(item => item.Context, StringComparer.Ordinal).ThenBy(item => item.Name, StringComparer.Ordinal)
            .Select(item => new TranslationItem
            {
                Context = item.Context,
                Key = item.Name,
                SourceValue = item.Value,
                Value = stored.GetValueOrDefault((item.Context, item.Name)),
                IsTranslated = stored.ContainsKey((item.Context, item.Name)),
            }).ToArray();
        return TypedResults.Ok(new TranslationListResponse { Culture = culture, Skip = skip, Take = take, TotalCount = items.Length, Items = items.Skip(skip).Take(take).ToArray() });
    }

    internal static async Task<IResult> SetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILocalizationService localization, [FromServices] ITranslationsManager manager,
        [FromServices] IEnumerable<ILocalizationDataProvider> providers, [FromBody] TranslationRequest request)
    {
        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(TranslationManagementEndpoints));

        var culture = await ResolveCultureAsync(localization, request?.Culture);
        if (culture is null || !ValidKey(request?.Context, request?.Key) || string.IsNullOrWhiteSpace(request?.Value) || request.Value.Length > 100_000)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["translation"] = [S["Provide a supported non-invariant culture, context and key of 1–1000 characters, and a nonblank value of at most 100000 characters. Use delete to remove a translation."]] });
        }

        if (!await CanEditAsync(context, authorization, culture))
        {
            return context.ApiForbidProblem();
        }

        var descriptors = await GetDescriptorsAsync(providers);
        if (!descriptors.Any(item => item.Context == request.Context && item.Name == request.Key))
        {
            return context.ApiNotFoundProblem(detail: S["No data localization provider registers this exact context and key."]);
        }

        var document = await manager.LoadTranslationsDocumentAsync();
        var translations = (document.Translations.GetValueOrDefault(culture) ?? []).ToList();
        var matching = translations.Where(item => item.Context == request.Context && item.Key == request.Key).ToArray();
        var changed = matching.Length != 1 || matching[0].Value != request.Value;
        if (changed)
        {
            translations.RemoveAll(item => item.Context == request.Context && item.Key == request.Key);
            translations.Add(new Translation { Context = request.Context, Key = request.Key, Value = request.Value });
            await manager.UpdateTranslationAsync(culture, translations);
        }

        return TypedResults.Ok(new TranslationResult { Culture = culture, Context = request.Context, Key = request.Key, Value = request.Value, Changed = changed });
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILocalizationService localization, [FromServices] ITranslationsManager manager, [AsParameters] TranslationKeyRequest request)
    {
        var S = context.RequestServices.GetRequiredService<IStringLocalizerFactory>().Create(typeof(TranslationManagementEndpoints));

        var culture = await ResolveCultureAsync(localization, request.Culture);
        if (culture is null || !ValidKey(request.Context, request.Key))
        {
            return context.ApiBadRequestProblem(detail: S["Provide a supported non-invariant culture and context and key of 1–1000 characters."]);
        }

        if (!await CanEditAsync(context, authorization, culture))
        {
            return context.ApiForbidProblem();
        }

        var document = await manager.LoadTranslationsDocumentAsync();
        var translations = (document.Translations.GetValueOrDefault(culture) ?? []).ToList();
        var changed = translations.RemoveAll(item => item.Context == request.Context && item.Key == request.Key) > 0;
        if (changed)
        {
            await manager.UpdateTranslationAsync(culture, translations);
        }

        return TypedResults.Ok(new TranslationResult { Culture = culture, Context = request.Context, Key = request.Key, Changed = changed });
    }

    private static bool ValidKey(string context, string key) =>
        !string.IsNullOrWhiteSpace(context) && context.Length <= 1000 && !string.IsNullOrWhiteSpace(key) && key.Length <= 1000;

    private static async Task<string> ResolveCultureAsync(ILocalizationService localization, string culture) =>
        string.IsNullOrEmpty(culture) ? null : (await localization.GetSupportedCulturesAsync())
            .FirstOrDefault(name => string.Equals(name, culture, StringComparison.OrdinalIgnoreCase));

    private static async Task<bool> CanEditAsync(HttpContext context, IAuthorizationService authorization, string culture) =>
        await authorization.AuthorizeAsync(context.User, DataLocalizationPermissions.ManageTranslations)
        || await authorization.AuthorizeAsync(context.User, DataLocalizationPermissions.CreateCulturePermission(culture, CultureInfo.GetCultureInfo(culture).DisplayName));

    private static async Task<DataLocalizedString[]> GetDescriptorsAsync(IEnumerable<ILocalizationDataProvider> providers)
    {
        var descriptors = new List<DataLocalizedString>();
        foreach (var provider in providers)
        {
            descriptors.AddRange(await provider.GetDescriptorsAsync());
        }

        return descriptors.DistinctBy(item => (item.Context, item.Name)).ToArray();
    }

#nullable enable
    internal sealed class TranslationRequest
    {
        [Required, Description("Supported culture name, for example fr or fr-FR.")]
        public string Culture { get; init; } = null!;

        [Required, StringLength(1000, MinimumLength = 1), Description("Exact context returned by translations list.")]
        public string Context { get; init; } = null!;

        [Required, StringLength(1000, MinimumLength = 1), Description("Exact key returned by translations list.")]
        public string Key { get; init; } = null!;

        [Required, StringLength(100000, MinimumLength = 1), Description("Translated text. Use delete to remove an existing translation.")]
        public string Value { get; init; } = null!;
    }

    internal sealed class TranslationKeyRequest
    {
        [FromQuery, Required]
        public string Culture { get; init; } = null!;

        [FromQuery, Required]
        public string Context { get; init; } = null!;

        [FromQuery, Required]
        public string Key { get; init; } = null!;
    }

    internal sealed class TranslationListRequest
    {
        [FromQuery, Required, Description("Supported culture name, for example fr or fr-FR.")]
        public string Culture { get; init; } = null!;

        [FromQuery, Description("Filter by context, key or source text (case-insensitive).")]
        public string? Search { get; init; }

        [FromQuery]
        public int? Skip { get; init; }

        [FromQuery]
        public int? Take { get; init; }
    }

    internal sealed class TranslationItem
    {
        public string Context { get; init; } = null!;

        public string Key { get; init; } = null!;

        public string? SourceValue { get; init; }

        public string? Value { get; init; }

        public bool IsTranslated { get; init; }
    }

    internal sealed class TranslationListResponse
    {
        public string Culture { get; init; } = null!;

        public int Skip { get; init; }

        public int Take { get; init; }

        public int TotalCount { get; init; }

        public TranslationItem[] Items { get; init; } = [];
    }

    internal sealed class TranslationResult
    {
        public string Culture { get; init; } = null!;

        public string Context { get; init; } = null!;

        public string Key { get; init; } = null!;

        public string? Value { get; init; }

        public bool Changed { get; init; }
    }
}
