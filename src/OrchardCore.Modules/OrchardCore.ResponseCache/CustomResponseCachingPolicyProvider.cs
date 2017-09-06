using Microsoft.AspNetCore.ResponseCaching.Internal;

namespace OrchardCore.ResponseCache
{
    public class CustomResponseCachingPolicyProvider : ResponseCachingPolicyProvider
    {
        public override bool AllowCacheLookup(ResponseCachingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return base.AllowCacheLookup(context);
        }

        public override bool AllowCacheStorage(ResponseCachingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return base.AllowCacheStorage(context);
        }

        public override bool AttemptResponseCaching(ResponseCachingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return base.AttemptResponseCaching(context);
        }

        public override bool IsCachedEntryFresh(ResponseCachingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return base.IsCachedEntryFresh(context);
        }

        public override bool IsResponseCacheable(ResponseCachingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return base.IsResponseCacheable(context);
        }
    }
}