namespace OrchardCore.Security
{
    public class ReferrerPolicyHeaderBuilder
    {
        private readonly SecurityHeadersBuilder _securityHeadersBuilder;

        public ReferrerPolicyHeaderBuilder(SecurityHeadersBuilder securityHeadersBuilder)
        {
            _securityHeadersBuilder = securityHeadersBuilder;
        }

        public SecurityHeadersBuilder WithNoReferrer()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.NoReferrer);

        public SecurityHeadersBuilder WithNoReferrerWhenDowngrade()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.NoReferrerWhenDowngrade);

        public SecurityHeadersBuilder WithOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.Origin);

        public SecurityHeadersBuilder WithOriginWhenCrossOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.OriginWhenCrossOrigin);

        public SecurityHeadersBuilder WithSameOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.SameOrigin);

        public SecurityHeadersBuilder WithStrictOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.StrictOrigin);

        public SecurityHeadersBuilder WithStrictOriginWhenCrossOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.StrictOriginWhenCrossOrigin);

        public SecurityHeadersBuilder WithUnsafeUrl()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyValue.UnsafeUrl);
    }
}
