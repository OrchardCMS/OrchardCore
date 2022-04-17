using Microsoft.Extensions.Primitives;

namespace OrchardCore.Security
{
    public class FrameOptionsValue
    {
        private readonly string _option;

        internal FrameOptionsValue(string option) => _option = option;

        public static readonly FrameOptionsValue Deny = new("DENY");

        public static readonly FrameOptionsValue SameOrigin = new("SAMEORIGIN");

        public static implicit operator StringValues(FrameOptionsValue option) => option.ToString();

        public static implicit operator string(FrameOptionsValue option) => option.ToString();

        public override string ToString() => _option;
    }
}
