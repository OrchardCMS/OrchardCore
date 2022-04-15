using System;

namespace OrchardCore.Security
{
    public static class SecurityHeadersBuilderExtensions
    {
        public static SecurityHeadersBuilder AddReferrerPolicyNoReferrer(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.NoReferrer);
        }

        public static SecurityHeadersBuilder AddReferrerPolicyNoReferrerWhenDowngrade(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.NoReferrerWhenDowngrade);
        }

        public static SecurityHeadersBuilder AddReferrerPolicyOrigin(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.Origin);
        }

        public static SecurityHeadersBuilder AddReferrerPolicyOriginWhenCrossOrigin(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.OriginWhenCrossOrigin);
        }

        public static SecurityHeadersBuilder AddReferrerPolicySameOrigin(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.SameOrigin);
        }

        public static SecurityHeadersBuilder AddReferrerPolicyStrictOrigin(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.StrictOrigin);
        }

        public static SecurityHeadersBuilder AddReferrerPolicyStrictOriginWhenCrossOrigin(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.StrictOriginWhenCrossOrigin);
        }

        public static SecurityHeadersBuilder AddReferrerPolicyUnsafeUrl(this SecurityHeadersBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddReferrerPolicy(ReferrerPolicyOptions.UnsafeUrl);
        }
    }
}
