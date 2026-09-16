using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Localization.Endpoints;

internal static partial class LocalizationManagementEndpoints
{
    private static void AddMessageEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/messages", MessagesAsync)
            .WithName("ApiListLocalizationMessages").WithSummary("Lists standard localization catalog entries.")
            .WithDescription("Reads loaded PO catalog entries for the selected culture without merging parent catalogs. Uses the request UI culture when omitted; explicit cultures can select a supported parent according to tenant settings. Filter by exact context/key or ordinal key prefix. Returns all stored plural forms. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "messages"], "list") { Capability = Capability })
            .Produces<MessageListResponse>().ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
        group.MapPost("/messages/resolve", ResolveMessageAsync)
            .WithName("ApiResolveLocalizationMessage").WithSummary("Resolves a standard localized message.")
            .WithDescription("Read-only lookup through the tenant string localizer, including configured fallback and plural rules. Count occupies {0}; additional arguments begin at {1}. Without count, arguments begin at {0}. Requires ManageCultures.")
            .WithCliCommand(new CliOperationMetadata(["localization", "messages"], "resolve") { Capability = Capability, InputMode = CliInputMode.Json })
            .Accepts<MessageResolveRequest>("application/json")
            .Produces<MessageResolveResponse>().ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> MessagesAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILocalizationService localization, [FromServices] ILocalizationManager manager, [AsParameters] MessageListRequest request)
    {
        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var culture = await MessageCultureAsync(localization, request.Culture);
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (culture is null || skip < 0 || take is < 1 or > 200 || request.Context?.Length > 1000 || request.Key?.Length > 1000 || request.Prefix?.Length > 1000)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["query"] = ["Use a supported culture, filters of at most 1000 characters, nonnegative skip, and take between 1 and 200."] });
        }

        var entries = manager.GetDictionary(CultureInfo.GetCultureInfo(culture)).Translations
            .Where(entry => (request.Context is null || string.Equals(entry.Key.Context ?? "", request.Context, StringComparison.Ordinal))
                && (request.Key is null || string.Equals(entry.Key.MessageId, request.Key, StringComparison.Ordinal))
                && (request.Prefix is null || entry.Key.MessageId.StartsWith(request.Prefix, StringComparison.Ordinal)))
            .OrderBy(entry => entry.Key.Context, StringComparer.Ordinal).ThenBy(entry => entry.Key.MessageId, StringComparer.Ordinal).ToArray();
        return TypedResults.Ok(new MessageListResponse
        {
            Culture = culture, Skip = skip, Take = take, TotalCount = entries.Length,
            Items = entries.Skip(skip).Take(take).Select(entry => new MessageEntry
            {
                Context = entry.Key.Context, Key = entry.Key.MessageId, Translations = entry.Value.ToArray(),
            }).ToArray(),
        });
    }

    internal static async Task<IResult> ResolveMessageAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILocalizationService localization, [FromServices] IStringLocalizerFactory factory, [FromBody] MessageResolveRequest request)
    {
        if (!await authorization.AuthorizeAsync(context.User, LocalizationPermissions.ManageCultures))
        {
            return context.ApiForbidProblem();
        }

        var culture = await MessageCultureAsync(localization, request?.Culture);
        if (culture is null || string.IsNullOrEmpty(request?.Key) || request.Key.Length > 1000 || request.Context?.Length > 1000
            || request.Count < 0 || (request.Count.HasValue != (request.Plural is not null))
            || request.Plural is { Length: 0 or > 1000 } || request.Arguments is { Length: > 16 }
            || request.Arguments?.Any(argument => argument.ValueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null)
                || argument.GetRawText().Length > 1000) == true)
        {
            return InvalidMessage("Use a supported culture, a nonempty key, context/key/plural of at most 1000 characters, paired nonnegative count and plural, and up to 16 scalar arguments of at most 1000 characters.");
        }

        using var scope = request.Culture is null ? null : CultureScope.Create(culture, ignoreSystemSettings: true);
        var localizer = factory.Create(request.Context ?? "", "");
        object[] arguments;
        try
        {
            arguments = (request.Arguments ?? []).Select(argument => argument.ValueKind switch
            {
                JsonValueKind.String => (object)argument.GetString(),
                JsonValueKind.Number => argument.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null,
            }).ToArray();
        }
        catch (FormatException)
        {
            return InvalidMessage("Numeric arguments must fit within a decimal value.");
        }

        LocalizedString template;
        if (request.Count.HasValue)
        {
            if (localizer is not IPluralStringLocalizer pluralLocalizer)
            {
                return InvalidMessage("The configured localizer does not support plural resolution.");
            }

            (template, arguments) = pluralLocalizer.GetTranslation(request.Key, new PluralizationArgument
            {
                Count = request.Count.Value, Forms = [request.Key, request.Plural], Arguments = arguments,
            });
        }
        else
        {
            template = localizer[request.Key];
        }

        // Bound alignment/precision as well as input size before invoking composite formatting.
        // Catalog entries and source fallbacks can both contain format directives.
        if (template.Value.Length > 10_000 || (arguments.Length > 0 && FormatItems().Matches(template.Value).SelectMany(item => FormatNumbers().Matches(item.Value)).Any(match =>
            !int.TryParse(match.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number > 1000)))
        {
            return InvalidMessage("The template exceeds formatting limits (10000 characters; numeric format components at most 1000).");
        }

        try
        {
            var value = arguments.Length == 0 ? template.Value : string.Format(CultureInfo.CurrentCulture, template.Value, arguments);
            return TypedResults.Ok(new MessageResolveResponse { Culture = culture, FormattingCulture = CultureInfo.CurrentCulture.Name, Context = request.Context, Key = request.Key, Count = request.Count, Template = template.Value, Value = value });
        }
        catch (FormatException)
        {
            return InvalidMessage("The selected localization template and arguments are not a valid composite format.");
        }
    }

    private static Microsoft.AspNetCore.Http.HttpResults.ValidationProblem InvalidMessage(string message) => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["message"] = [message] });

    private static async Task<string> MessageCultureAsync(ILocalizationService localization, string requested)
    {
        if (requested is null)
        {
            // Request localization has already evaluated the tenant's configured providers.
            return CultureInfo.CurrentUICulture.Name;
        }

        if (requested.Length > 100)
        {
            return null;
        }

        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(requested);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }

        var supported = await localization.GetSupportedCulturesAsync();
        while (true)
        {
            var match = supported.FirstOrDefault(name => string.Equals(name, culture.Name, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }

            if (!localization.FallBackToParentCultures || culture.Equals(CultureInfo.InvariantCulture))
            {
                return null;
            }

            culture = culture.Parent;
        }
    }

    [GeneratedRegex(@"\{[^{}]*\}", RegexOptions.CultureInvariant)]
    private static partial Regex FormatItems();

    [GeneratedRegex("[0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex FormatNumbers();

#nullable enable
    internal sealed class MessageListRequest
    {
        [FromQuery, Description("Explicit culture, optionally selecting a supported parent according to tenant settings. Omit to use the resolved request UI culture. Catalog entries are not merged with parent catalogs.")]
        public string? Culture { get; init; }
        [FromQuery, Description("Exact ordinal PO context. Omit to include all contexts; empty selects contextless entries.")]
        public string? Context { get; init; }
        [FromQuery, Description("Exact ordinal msgid.")]
        public string? Key { get; init; }
        [FromQuery, Description("Ordinal, case-sensitive msgid prefix. Combined with other filters using AND.")]
        public string? Prefix { get; init; }
        [FromQuery] public int? Skip { get; init; }
        [FromQuery] public int? Take { get; init; }
    }

    internal sealed class MessageResolveRequest
    {
        [Description("Explicit culture, optionally selecting a supported parent according to tenant settings. Omit to use the resolved request culture and formatting culture.")]
        public string? Culture { get; init; }
        [Description("PO context, matching the localizer base name. Omit for contextless lookup.")]
        public string? Context { get; init; }
        [Required, Description("Source msgid, also used when no translation exists.")]
        public string Key { get; init; } = null!;
        [Description("Nonnegative plural count, paired with plural. Inserted as argument {0}.")]
        public int? Count { get; init; }
        [Description("Source plural fallback, required with count.")]
        public string? Plural { get; init; }
        [Description("Up to 16 strings, decimal numbers, booleans or nulls. With count, these start at {1}; otherwise at {0}.")]
        public JsonElement[]? Arguments { get; init; }
    }

    internal sealed class MessageEntry
    {
        public string? Context { get; init; }
        public string Key { get; init; } = null!;
        public string[] Translations { get; init; } = [];
    }
    internal sealed class MessageListResponse
    {
        public string Culture { get; init; } = null!;
        public int Skip { get; init; }
        public int Take { get; init; }
        public int TotalCount { get; init; }
        public MessageEntry[] Items { get; init; } = [];
    }
    internal sealed class MessageResolveResponse
    {
        public string Culture { get; init; } = null!;
        public string FormattingCulture { get; init; } = null!;
        public string? Context { get; init; }
        public string Key { get; init; } = null!;
        public int? Count { get; init; }
        public string Template { get; init; } = null!;
        public string Value { get; init; } = null!;
    }
}
