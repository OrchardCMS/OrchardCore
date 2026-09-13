using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

/// <summary>Validates built-in source definitions consistently for administration and remote management.</summary>
public static class SitemapSourceValidation
{
    /// <summary>Returns errors for an invalid built-in source without changing the definition.</summary>
    public static Dictionary<string, string[]> Validate(SitemapSource source)
    {
        var errors = new Dictionary<string, string[]>();
        if (source is CustomPathSitemapSource custom)
        {
            if (string.IsNullOrWhiteSpace(custom.Path) || custom.Path.Length > CustomPathSitemapSource.MaxPathLength
                || custom.Path.IndexOfAny(CustomPathSitemapSource.InvalidCharactersForPath) >= 0
                || custom.Path.Any(char.IsWhiteSpace) || custom.Path.Contains("//", StringComparison.Ordinal))
            {
                errors["path"] = ["Provide a relative path of at most 1024 characters without spaces, reserved characters or consecutive slashes."];
            }
            ValidateValues(custom.Priority, custom.ChangeFrequency, "", errors);
        }
        else if (source is ContentTypesSitemapSource content)
        {
            ValidateValues(content.Priority, content.ChangeFrequency, "", errors);
            if (content.ContentTypes is null || content.ContentTypes.Any(entry => entry is null || string.IsNullOrWhiteSpace(entry.ContentTypeName)))
            {
                errors["contentTypes"] = ["Provide named content type entries."];
            }
            else
            {
                foreach (var entry in content.ContentTypes) { ValidateValues(entry.Priority, entry.ChangeFrequency, "contentTypes.", errors); }
                if (content.ContentTypes.Select(entry => entry.ContentTypeName).Distinct(StringComparer.Ordinal).Count() != content.ContentTypes.Length)
                {
                    errors["contentTypes"] = ["Content types must be distinct."];
                }
            }
            if (content.LimitItems && !content.IndexAll)
            {
                var limited = content.LimitedContentType;
                if (limited is null || string.IsNullOrWhiteSpace(limited.ContentTypeName) || limited.Skip < 0 || limited.Take < 1)
                {
                    errors["limitedContentType"] = ["Select a content type with nonnegative skip and positive take."];
                }
                else { ValidateValues(limited.Priority, limited.ChangeFrequency, "limitedContentType.", errors); }
            }
        }
        else { errors["type"] = ["This source type has no typed management contract."]; }
        return errors;
    }

    private static void ValidateValues(int priority, ChangeFrequency frequency, string prefix, Dictionary<string, string[]> errors)
    {
        if (priority < 0 || priority > 10) { errors[prefix + "priority"] = ["Priority must be between zero and ten."]; }
        if (!Enum.IsDefined(frequency)) { errors[prefix + "changeFrequency"] = ["Select a supported change frequency."]; }
    }
}
