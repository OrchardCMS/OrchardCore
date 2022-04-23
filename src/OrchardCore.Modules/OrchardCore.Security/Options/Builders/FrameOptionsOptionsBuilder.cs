namespace OrchardCore.Security.Options
{
    public class FrameOptionsOptionsBuilder
    {
        private readonly FrameOptionsOptions _options;

        public FrameOptionsOptionsBuilder(FrameOptionsOptions options)
            => _options = options;

        public FrameOptionsOptions WithDeny()
        {
            _options.Option = FrameOptionsValue.Deny;

            return _options;
        }

        public FrameOptionsOptions WithSameOrigin()
        {
            _options.Option = FrameOptionsValue.SameOrigin;

            return _options;
        }
    }
}
