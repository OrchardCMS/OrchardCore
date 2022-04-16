using System;

namespace OrchardCore.Security
{
    public static class SecurityHeadersBuilderExtensions
    {
        public static ReferrerPolicyHeaderBuilder AddReferrerPolicy(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return new ReferrerPolicyHeaderBuilder(builder);
        }

        public static FrameOptionsHeaderBuilder AddFrameOptions(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return new FrameOptionsHeaderBuilder(builder);
        }

        public static PermissionsPolicyHeaderBuilder AddPermissionsPolicy(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return new PermissionsPolicyHeaderBuilder(builder);
        }
    }
}
