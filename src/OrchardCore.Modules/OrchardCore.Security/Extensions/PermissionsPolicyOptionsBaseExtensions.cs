using System;

namespace OrchardCore.Security
{
    public static class PermissionsPolicyOptionsBaseExtensions
    {
        public static void AllowAnyOrigin(this PermissionsPolicyOptionsBase options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Origin = PermissionsPolicyOriginValue.Any;
        }

        public static void AllowSelfOrigin(this PermissionsPolicyOptionsBase options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Origin = PermissionsPolicyOriginValue.Self;
        }
    }
}
