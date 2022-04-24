using System;

namespace OrchardCore.Security.Options.Builders
{
    public class FrameAncestorsContentSecurityPolicyOptionsBuilder
    {
        private readonly FrameAncestorsContentSecurityPolicyOptions _options;

        public FrameAncestorsContentSecurityPolicyOptionsBuilder(FrameAncestorsContentSecurityPolicyOptions options)
            => _options = options;

        public FrameAncestorsContentSecurityPolicyOptionsBuilder FromAny()
        {
            //_options.Origin = ContentSecurityPolicyOriginValue.Any;
            //_options.Value = $"{_options.Name} {_options.Origin}";

            return this;
        }

        public FrameAncestorsContentSecurityPolicyOptionsBuilder FromNone()
        {
            //_options.Origin = ContentSecurityPolicyOriginValue.None;
            //.Value = $"{_options.Name} {_options.Origin}";

            return this;
        }

        public FrameAncestorsContentSecurityPolicyOptionsBuilder FromSelf(params string[] allowedOrigins)
        {
            //_options.Origin = ContentSecurityPolicyOriginValue.Self;
            //_options.Value = allowedOrigins.Length == 0
            //    ? $"{_options.Name} {_options.Origin}"
            //    : $"{_options.Name} {_options.Origin} {String.Join(' ', allowedOrigins)}";

            return this;
        }
    }
}
