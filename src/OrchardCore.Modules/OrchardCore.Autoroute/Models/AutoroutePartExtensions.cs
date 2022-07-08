using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Autoroute.Models
{
    public static class AutoroutePartExtensions
    {
        public static IEnumerable<ValidationResult> ValidatePathFieldValue(this AutoroutePart autoroute, IStringLocalizer S)
        {
            if (autoroute.Path == "/")
            {
                yield return new ValidationResult(S["Your permalink can't be set to the homepage, please use the homepage option instead."], new[] { nameof(autoroute.Path) });
            }

            if (HasInvalidCharacters(autoroute.Path))
            {
                var invalidCharactersForMessage = string.Join(", ", AutoroutePart.InvalidCharactersForPath.Select(c => $"\"{c}\""));
                yield return new ValidationResult(S["Please do not use any of the following characters in your permalink: {0}. No spaces, or consecutive slashes, are allowed (please use dashes or underscores instead).", invalidCharactersForMessage], new[] { nameof(autoroute.Path) });
            }

            if (autoroute.Path?.Length > AutoroutePart.MaxPathLength)
            {
                yield return new ValidationResult(S["Your permalink is too long. The permalink can only be up to {0} characters.", AutoroutePart.MaxPathLength], new[] { nameof(autoroute.Path) });
            }
        }

        private static bool HasInvalidCharacters(string path)
        {
            // IndexOfAny performs culture-insensitive and case-sensitive search.
            if (path?.IndexOfAny(AutoroutePart.InvalidCharactersForPath) > -1)
            {
                return true;
            }

            if (path?.IndexOf(' ', StringComparison.Ordinal) > -1)
            {
                return true;
            }

            if (path?.IndexOf("//", StringComparison.Ordinal) > -1)
            {
                return true;
            }

            return false;
        }
    }
}
