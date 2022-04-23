namespace OrchardCore.Security.Options
{
    public class ReferrerPolicyOptionsBuilder
    {
        private readonly ReferrerPolicyOptions _options;

        public ReferrerPolicyOptionsBuilder(ReferrerPolicyOptions options)
            => _options = options;

        public ReferrerPolicyOptions WithNoReferrer()
        {
            _options.Policy = ReferrerPolicyValue.NoReferrer;

            return _options;
        }

        public ReferrerPolicyOptions WithNoReferrerWhenDowngrade()
        {
            _options.Policy = ReferrerPolicyValue.NoReferrerWhenDowngrade;

            return _options;
        }

        public ReferrerPolicyOptions WithOrigin()
        {
            _options.Policy = ReferrerPolicyValue.Origin;

            return _options;
        }

        public ReferrerPolicyOptions WithOriginWhenCrossOrigin()
        {
            _options.Policy = ReferrerPolicyValue.OriginWhenCrossOrigin;

            return _options;
        }

        public ReferrerPolicyOptions WithSameOrigin()
        {
            _options.Policy = ReferrerPolicyValue.SameOrigin;

            return _options;
        }

        public ReferrerPolicyOptions WithStrictOrigin()
        {
            _options.Policy = ReferrerPolicyValue.StrictOrigin;

            return _options;
        }

        public ReferrerPolicyOptions WithStrictOriginWhenCrossOrigin()
        {
            _options.Policy = ReferrerPolicyValue.StrictOriginWhenCrossOrigin;

            return _options;
        }

        public ReferrerPolicyOptions WithUnsafeUrl()
        {
            _options.Policy = ReferrerPolicyValue.UnsafeUrl;

            return _options;
        }
    }
}
