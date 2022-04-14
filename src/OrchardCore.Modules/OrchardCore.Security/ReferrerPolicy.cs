using Microsoft.Extensions.Primitives;

namespace OrchardCore.Security
{
    public class ReferrerPolicy
    {
        private readonly string _policy;

        public static readonly ReferrerPolicy NoReferrer = new("no-referrer");

        public static readonly ReferrerPolicy NoReferrerWhenDowngrade = new("no-referrer-when-downgrade");

        public static readonly ReferrerPolicy Origin = new("origin");

        public static readonly ReferrerPolicy OriginWhenCrossOrigin = new("origin-when-cross-origin");

        public static readonly ReferrerPolicy SameOrigin = new("same-origin");

        public static readonly ReferrerPolicy StrictOrigin = new("strict-origin");

        public static readonly ReferrerPolicy StrictOriginWhenCrossOrigin = new("strict-origin-when-cross-origin");

        public static readonly ReferrerPolicy UnsafeUrl = new("unsafe-url");

        private ReferrerPolicy(string policy) => _policy = policy;

        public static implicit operator StringValues(ReferrerPolicy policy) => policy.ToString();

        public override string ToString() => _policy;
    }
}
