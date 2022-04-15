namespace OrchardCore.Security
{
    public class FrameOptionsHeaderBuilder
    {
        private readonly SecurityHeadersBuilder _securityHeadersBuilder;

        public FrameOptionsHeaderBuilder(SecurityHeadersBuilder securityHeadersBuilder)
        {
            _securityHeadersBuilder = securityHeadersBuilder;
        }

        public SecurityHeadersBuilder WithDeny()
            => _securityHeadersBuilder.AddFrameOptions(FrameOptions.Deny);

        public SecurityHeadersBuilder WithSameOrigin()
            => _securityHeadersBuilder.AddFrameOptions(FrameOptions.SameOrigin);
    }
}
