using System;

namespace OrchardCore.Security.Options
{
    public class ReferrerPolicyOptionsBuilder
    {
        private readonly ReferrerPolicyOptions _options;

        public ReferrerPolicyOptionsBuilder(ReferrerPolicyOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public ReferrerPolicyOptionsBuilder WithNoReferrer()
        {
            _options.Value = ReferrerPolicyValue.NoReferrer;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithNoReferrerWhenDowngrade()
        {
            _options.Value = ReferrerPolicyValue.NoReferrerWhenDowngrade;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithOrigin()
        {
            _options.Value = ReferrerPolicyValue.Origin;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithOriginWhenCrossOrigin()
        {
            _options.Value = ReferrerPolicyValue.OriginWhenCrossOrigin;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithSameOrigin()
        {
            _options.Value = ReferrerPolicyValue.SameOrigin;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithStrictOrigin()
        {
            _options.Value = ReferrerPolicyValue.StrictOrigin;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithStrictOriginWhenCrossOrigin()
        {
            _options.Value = ReferrerPolicyValue.StrictOriginWhenCrossOrigin;

            return this;
        }

        public ReferrerPolicyOptionsBuilder WithUnsafeUrl()
        {
            _options.Value = ReferrerPolicyValue.UnsafeUrl;

            return this;
        }
    }
}
