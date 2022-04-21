using System;
using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public static class PermissionsPolicyOptionsBaseExtensions
    {
        public static void AllowAnyOrigin(this PermissionsPolicyOptionsBase options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            options.Origin = PermissionsPolicyOriginValue.Any;
        }

        public static void AllowSelfOrigin(this PermissionsPolicyOptionsBase options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            options.Origin = PermissionsPolicyOriginValue.Self;
        }
    }
}
