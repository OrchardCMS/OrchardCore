using Microsoft.Extensions.Localization;
using OrchardCore.Security.Options;
using OrchardCore.Security.Settings;

namespace OrchardCore.Security.Services;

internal static class SecuritySettingsEditor
{
    internal static readonly string[] ReferrerPolicies =
    [
        ReferrerPolicyValue.NoReferrer, ReferrerPolicyValue.NoReferrerWhenDowngrade,
        ReferrerPolicyValue.Origin, ReferrerPolicyValue.OriginWhenCrossOrigin,
        ReferrerPolicyValue.SameOrigin, ReferrerPolicyValue.StrictOrigin,
        ReferrerPolicyValue.StrictOriginWhenCrossOrigin, ReferrerPolicyValue.UnsafeUrl,
    ];

    internal static Dictionary<string, string[]> Validate(SecuritySettings settings, IStringLocalizer localizer)
    {
        var errors = new Dictionary<string, string[]>();
        ValidatePolicy(settings.ContentSecurityPolicy, nameof(settings.ContentSecurityPolicy), ';', errors, localizer);
        ValidatePolicy(settings.PermissionsPolicy, nameof(settings.PermissionsPolicy), ',', errors, localizer);
        if (!ReferrerPolicies.Contains(settings.ReferrerPolicy, StringComparer.Ordinal))
        {
            errors[nameof(settings.ReferrerPolicy)] = [localizer["Select a valid referrer policy."]];
        }
        return errors;
    }

    private static void ValidatePolicy(Dictionary<string, string> policy, string property, char separator,
        Dictionary<string, string[]> errors, IStringLocalizer localizer)
    {
        foreach (var (name, value) in policy)
        {
            if (string.IsNullOrEmpty(name) || !char.IsAsciiLetter(name[0])
                || name.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
            {
                errors[property] = [localizer["Directive names must start with a letter and contain only ASCII letters, digits or hyphens."]];
            }
            if (value is not null && value.Any(character => character < ' ' || character > '~' || character == separator))
            {
                errors[property + "." + name] = [localizer["Use printable ASCII directive values without header control characters or directive separators."]];
            }
            if (property == nameof(SecuritySettings.PermissionsPolicy) && value is null)
            {
                errors[property + "." + name] = [localizer["A permissions policy directive requires a string value."]];
            }
        }
    }

    internal static bool Apply(SecuritySettings settings, SecuritySettings proposed)
    {
        if (settings.ContentTypeOptions == proposed.ContentTypeOptions
            && settings.ReferrerPolicy == proposed.ReferrerPolicy
            && Equal(settings.ContentSecurityPolicy, proposed.ContentSecurityPolicy)
            && Equal(settings.PermissionsPolicy, proposed.PermissionsPolicy))
        {
            return false;
        }
        settings.ContentTypeOptions = proposed.ContentTypeOptions;
        settings.ReferrerPolicy = proposed.ReferrerPolicy;
        settings.ContentSecurityPolicy = proposed.ContentSecurityPolicy;
        settings.PermissionsPolicy = proposed.PermissionsPolicy;
        return true;
    }

    private static bool Equal(Dictionary<string, string> first, Dictionary<string, string> second)
        => first.Count == second.Count && first.All(entry => second.TryGetValue(entry.Key, out var value) && entry.Value == value);
}
