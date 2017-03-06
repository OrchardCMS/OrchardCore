using System;
using Orchard.Environment.Extensions;

namespace Orchard.DisplayManagement
{
    public static class ManifestExtensions
    {
        public static bool IsModule(this IManifestInfo manifestInfo)
        {
            return manifestInfo?.Type?.Equals("module", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public static bool IsTheme(this IManifestInfo manifestInfo)
        {
            return manifestInfo?.Type?.Equals("theme", StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }
}