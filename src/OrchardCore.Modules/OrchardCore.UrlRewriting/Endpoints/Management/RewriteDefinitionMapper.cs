using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.UrlRewriting.Endpoints.Management;

internal static class RewriteDefinitionMapper
{
    internal static bool Supported(string source) => source is UrlRewriteRuleSource.SourceName or UrlRedirectRuleSource.SourceName;

    internal static Dictionary<string, string[]> Apply(RewriteRule rule, RewriteDefinition definition)
    {
        var errors = new Dictionary<string, string[]>();
        if (definition is null)
        {
            errors["body"] = ["A complete rule definition is required."];
            return errors;
        }
        if (!Supported(definition.Source) || definition.Source != rule.Source)
        {
            errors["source"] = ["Use an available built-in Rewrite or Redirect source; an existing source cannot change."];
        }
        if (definition.Id is not null && (!ValidId(definition.Id) || definition.Id != rule.Id))
        {
            errors["id"] = ["The identifier must match the existing rule and contain 1–128 ASCII letters, digits, underscores or hyphens."];
        }
        if (!TryEnum(definition.QueryStringPolicy, out QueryStringPolicy query))
        {
            errors["queryStringPolicy"] = ["Use Append or Drop."];
        }
        rule.Name = definition.Name;
        if (rule.Source == UrlRewriteRuleSource.SourceName)
        {
            if (definition.RedirectType is not null)
            {
                errors["redirectType"] = ["Redirect status is only valid for Redirect rules."];
            }
            rule.Put(new UrlRewriteSourceMetadata
            {
                Pattern = definition.Pattern, SubstitutionPattern = definition.SubstitutionPattern,
                IsCaseInsensitive = definition.IsCaseInsensitive, QueryStringPolicy = query,
                SkipFurtherRules = definition.SkipFurtherRules ?? false,
            });
        }
        else if (rule.Source == UrlRedirectRuleSource.SourceName)
        {
            if (definition.SkipFurtherRules.HasValue)
            {
                errors["skipFurtherRules"] = ["Skip further rules is only valid for Rewrite rules."];
            }
            if (!TryEnum(definition.RedirectType ?? nameof(RedirectType.Found), out RedirectType redirect))
            {
                errors["redirectType"] = ["Use Found, MovedPermanently, TemporaryRedirect or PermanentRedirect."];
            }
            rule.Put(new UrlRedirectSourceMetadata
            {
                Pattern = definition.Pattern, SubstitutionPattern = definition.SubstitutionPattern,
                IsCaseInsensitive = definition.IsCaseInsensitive, QueryStringPolicy = query, RedirectType = redirect,
            });
        }
        return errors;
    }

    internal static RewriteResponse Describe(RewriteRule rule)
    {
        RewriteDefinition definition = null;
        if (rule.Source == UrlRewriteRuleSource.SourceName)
        {
            var metadata = rule.TryGet<UrlRewriteSourceMetadata>(out var rewrite) ? rewrite : new UrlRewriteSourceMetadata();
            definition = new()
            {
                Id = rule.Id, Name = rule.Name, Source = rule.Source, Pattern = metadata.Pattern,
                SubstitutionPattern = metadata.SubstitutionPattern, IsCaseInsensitive = metadata.IsCaseInsensitive,
                QueryStringPolicy = metadata.QueryStringPolicy.ToString(), SkipFurtherRules = metadata.SkipFurtherRules,
            };
        }
        else if (rule.Source == UrlRedirectRuleSource.SourceName)
        {
            var metadata = rule.TryGet<UrlRedirectSourceMetadata>(out var redirect) ? redirect : new UrlRedirectSourceMetadata();
            definition = new()
            {
                Id = rule.Id, Name = rule.Name, Source = rule.Source, Pattern = metadata.Pattern,
                SubstitutionPattern = metadata.SubstitutionPattern, IsCaseInsensitive = metadata.IsCaseInsensitive,
                QueryStringPolicy = metadata.QueryStringPolicy.ToString(), RedirectType = metadata.RedirectType.ToString(),
            };
        }
        return new() { Id = rule.Id, Name = rule.Name, Source = rule.Source, Order = rule.Order,
            CreatedUtc = rule.CreatedUtc, IsWritable = definition is not null, Definition = definition,
        };
    }

    internal static bool ValidId(string id) => id is { Length: > 0 and <= 128 }
        && id.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');

    private static bool TryEnum<T>(string value, out T result) where T : struct, Enum
    {
        result = default;
        return Enum.GetNames<T>().Contains(value, StringComparer.OrdinalIgnoreCase)
            && Enum.TryParse(value, ignoreCase: true, out result);
    }
}
