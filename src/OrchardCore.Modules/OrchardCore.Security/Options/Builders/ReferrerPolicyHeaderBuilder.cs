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
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.NoReferrer);

        public SecurityHeadersBuilder WithNoReferrerWhenDowngrade()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.NoReferrerWhenDowngrade);

        public SecurityHeadersBuilder WithOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.Origin);

        public SecurityHeadersBuilder WithOriginWhenCrossOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.OriginWhenCrossOrigin);

        public SecurityHeadersBuilder WithSameOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.SameOrigin);

        public SecurityHeadersBuilder WithStrictOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.StrictOrigin);

        public SecurityHeadersBuilder WithStrictOriginWhenCrossOrigin()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.StrictOriginWhenCrossOrigin);

        public SecurityHeadersBuilder WithUnsafeUrl()
            => _securityHeadersBuilder.AddReferrerPolicy(ReferrerPolicyOptions.UnsafeUrl);
    }
}
