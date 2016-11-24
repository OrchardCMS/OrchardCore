using Orchard.Environment.Extensions;
using System;

namespace Orchard.Modules
{
    public static class ManifestExtensions
    {
        public static bool IsModule(this IManifestInfo manifestInfo)
        {
            return manifestInfo.Type.Equals("module", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTheme(this IManifestInfo manifestInfo)
        {
            return manifestInfo.Type.Equals("theme", StringComparison.OrdinalIgnoreCase);
        }
    }
}
