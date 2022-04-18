using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Security
{
    public static class SecurityHeadersBuilderExtensions
    {
        public static SecurityHeadersBuilder AddFrameOptions(this SecurityHeadersBuilder builder, FrameOptionsValue value)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return builder.AddFrameOptions(value);
        }

        public static SecurityHeadersBuilder AddReferrerPolicy(this SecurityHeadersBuilder builder, ReferrerPolicyValue policy)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (policy is null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            return builder.AddReferrerPolicy(policy);
        }

        public static SecurityHeadersBuilder AddPermissionsPolicy(this SecurityHeadersBuilder builder, Action<PermissionsPolicyOptions> action)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var options = new PermissionsPolicyOptions();

            action.Invoke(options);

            return builder.AddPermissionsPolicy(options);
        }

        public static SecurityHeadersBuilder AddStrictTransportSecurity(this SecurityHeadersBuilder builder, Action<StrictTransportSecurityOptions> action)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var options = new StrictTransportSecurityOptions();

            action.Invoke(options);

            return builder.AddStrictTransportSecurity(options);
        }
    }
}
