using System;

namespace OrchardCore.Security.Options
{
    public class FrameOptionsOptionsBuilder
    {
        private readonly FrameOptionsOptions _options;

        public FrameOptionsOptionsBuilder(FrameOptionsOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public FrameOptionsOptionsBuilder WithDeny()
        {
            _options.Value = FrameOptionsValue.Deny;

            return this;
        }

        public FrameOptionsOptionsBuilder WithSameOrigin()
        {
            _options.Value = FrameOptionsValue.SameOrigin;

            return this;
        }
    }
}
