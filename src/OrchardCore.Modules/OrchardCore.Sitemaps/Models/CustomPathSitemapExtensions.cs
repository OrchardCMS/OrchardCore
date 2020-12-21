using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Sitemaps.Models
{
    public static class CustomPathSitemapExtensions
    {
        /// <summary>
        /// some validation checks are used in AutoroutePartDisplayDriver (see AutoroutePartExtensions)
        /// </summary>
        public static IEnumerable<ValidationResult> ValidateUrlFieldValue(this CustomPathSitemapSource sitemap, IStringLocalizer S)
        {
            if (sitemap.Url?.IndexOfAny(CustomPathSitemapSource.InvalidCharactersForPath) > -1 || sitemap.Url?.IndexOf(' ') > -1 || sitemap.Url?.IndexOf("//") > -1)
            {
                var invalidCharactersForMessage = string.Join(", ", CustomPathSitemapSource.InvalidCharactersForPath.Select(c => $"\"{c}\""));
                yield return new ValidationResult(S["Please do not use any of the following characters in your path: {0}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", invalidCharactersForMessage], new[] { nameof(sitemap.Url) });
            }

            if (sitemap.Url?.Length > CustomPathSitemapSource.MaxPathLength)
            {
                yield return new ValidationResult(S["Your path is too long. The path can only be up to {0} characters.", CustomPathSitemapSource.MaxPathLength], new[] { nameof(sitemap.Url) });
            }
        }
    }
}