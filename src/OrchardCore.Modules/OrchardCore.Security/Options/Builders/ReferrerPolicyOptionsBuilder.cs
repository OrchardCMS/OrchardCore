namespace OrchardCore.Security.Options
{
    public class ReferrerPolicyOptionsBuilder
    {
        private readonly SecurityHeadersOptions _options;

        public ReferrerPolicyOptionsBuilder(SecurityHeadersOptions options)
            => _options = options;

        public SecurityHeadersOptions WithNoReferrer()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.NoReferrer;

            return _options;
        }

        public SecurityHeadersOptions WithNoReferrerWhenDowngrade()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.NoReferrerWhenDowngrade;

            return _options;
        }

        public SecurityHeadersOptions WithOrigin()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.Origin;

            return _options;
        }

        public SecurityHeadersOptions WithOriginWhenCrossOrigin()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.OriginWhenCrossOrigin;

            return _options;
        }

        public SecurityHeadersOptions WithSameOrigin()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.SameOrigin;

            return _options;
        }

        public SecurityHeadersOptions WithStrictOrigin()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.StrictOrigin;

            return _options;
        }

        public SecurityHeadersOptions WithStrictOriginWhenCrossOrigin()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.StrictOriginWhenCrossOrigin;

            return _options;
        }

        public SecurityHeadersOptions WithUnsafeUrl()
        {
            _options.ReferrerPolicy = ReferrerPolicyValue.UnsafeUrl;

            return _options;
        }
    }
}
