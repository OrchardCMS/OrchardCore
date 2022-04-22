namespace OrchardCore.Security.Options
{
    public class FrameOptionsOptionsBuilder
    {
        private readonly SecurityHeadersOptions _options;

        public FrameOptionsOptionsBuilder(SecurityHeadersOptions options)
            => _options = options;

        public SecurityHeadersOptions WithDeny()
        {
            _options.FrameOptions = FrameOptionsValue.Deny;

            return _options;
        }

        public SecurityHeadersOptions WithSameOrigin()
        {
            _options.FrameOptions = FrameOptionsValue.SameOrigin;

            return _options;
        }
    }
}
