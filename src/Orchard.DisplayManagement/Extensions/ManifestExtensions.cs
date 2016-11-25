using Orchard.Environment.Extensions;
using System;

namespace Orchard.DisplayManagement
{
    public static class ManifestExtensions
    {
        public static bool IsModule(this IManifestInfo manifestInfo)
        {
            return manifestInfo.Type?.Equals("module", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public static bool IsTheme(this IManifestInfo manifestInfo)
        {
            return manifestInfo.Type?.Equals("theme", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public static bool IsCore(this IManifestInfo manifestInfo)
        {
            return string.IsNullOrEmpty(manifestInfo.Type);
        }
    }
}
