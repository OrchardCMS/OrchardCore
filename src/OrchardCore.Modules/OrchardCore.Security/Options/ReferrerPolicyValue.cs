namespace OrchardCore.Security.Options
{
    public class ReferrerPolicyValue
    {
        public const string NoReferrer = "no-referrer";

        public const string NoReferrerWhenDowngrade = "no-referrer-when-downgrade";

        public const string Origin = "origin";

        public const string OriginWhenCrossOrigin = "origin-when-cross-origin";

        public const string SameOrigin = "same-origin";

        public const string StrictOrigin = "strict-origin";

        public const string StrictOriginWhenCrossOrigin = "strict-origin-when-cross-origin";

        public const string UnsafeUrl = "unsafe-url";
    }
}
