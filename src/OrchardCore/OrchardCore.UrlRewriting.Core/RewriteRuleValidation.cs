using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

internal static class RewriteRuleValidation
{
    internal static void Validate(RewriteValidateResult result, string pattern, string substitution,
        QueryStringPolicy queryStringPolicy, IStringLocalizer localizer, RedirectType? redirectType = null)
    {
        if (string.IsNullOrWhiteSpace(pattern) || !PatternHelper.IsValidRegex(pattern))
        {
            result.Fail(new ValidationResult(localizer["A valid Match URL Pattern is required."], ["Pattern"]));
        }
        if (string.IsNullOrWhiteSpace(substitution))
        {
            result.Fail(new ValidationResult(localizer["The Substitution URL Pattern is required."], ["SubstitutionPattern"]));
        }
        // Both values are emitted as single, unquoted Apache RewriteRule arguments.
        // Whitespace would create extra arguments or another directive.
        if (pattern?.Any(char.IsWhiteSpace) == true || pattern?.Any(char.IsControl) == true)
        {
            result.Fail(new ValidationResult(localizer["Use escaped characters instead of literal whitespace or control characters in the match pattern."], ["Pattern"]));
        }
        if (substitution?.Any(char.IsWhiteSpace) == true || substitution?.Any(char.IsControl) == true)
        {
            result.Fail(new ValidationResult(localizer["Use URL-encoded characters instead of literal whitespace or control characters in the substitution."], ["SubstitutionPattern"]));
        }
        if (!Enum.IsDefined(queryStringPolicy))
        {
            result.Fail(new ValidationResult(localizer["Select a valid query string policy."], ["QueryStringPolicy"]));
        }
        if (redirectType.HasValue && !Enum.IsDefined(redirectType.Value))
        {
            result.Fail(new ValidationResult(localizer["Select a valid redirect status."], ["RedirectType"]));
        }
    }
}
