using Microsoft.Extensions.Primitives;

namespace OrchardCore.Security.Options
{
    public class ReferrerPolicyValue
    {
        private readonly string _policy;

        internal ReferrerPolicyValue(string policy) => _policy = policy;

        public static readonly ReferrerPolicyValue NoReferrer = new("no-referrer");

        public static readonly ReferrerPolicyValue NoReferrerWhenDowngrade = new("no-referrer-when-downgrade");

        public static readonly ReferrerPolicyValue Origin = new("origin");

        public static readonly ReferrerPolicyValue OriginWhenCrossOrigin = new("origin-when-cross-origin");

        public static readonly ReferrerPolicyValue SameOrigin = new("same-origin");

        public static readonly ReferrerPolicyValue StrictOrigin = new("strict-origin");

        public static readonly ReferrerPolicyValue StrictOriginWhenCrossOrigin = new("strict-origin-when-cross-origin");

        public static readonly ReferrerPolicyValue UnsafeUrl = new("unsafe-url");

        public static implicit operator StringValues(ReferrerPolicyValue policy) => policy.ToString();

        public static implicit operator string(ReferrerPolicyValue policy) => policy.ToString();

        public override string ToString() => _policy;
    }
}
