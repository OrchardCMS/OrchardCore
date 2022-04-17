namespace OrchardCore.Security
{
    public class ContentTypeOptionsHeaderBuilder
    {
        private readonly SecurityHeadersBuilder _securityHeadersBuilder;

        public ContentTypeOptionsHeaderBuilder(SecurityHeadersBuilder securityHeadersBuilder)
        {
            _securityHeadersBuilder = securityHeadersBuilder;
        }

        public SecurityHeadersBuilder WithNoSniff()
            => _securityHeadersBuilder.AddFrameOptions(ContentTypeOptionsValue.NoSniff);
    }
}
