using Microsoft.AspNetCore.ResponseCaching.Internal;

namespace Orchard.ResponseCache
{
    public class CustomResponseCachingPolicyProvider : ResponseCachingPolicyProvider
    {
        public override bool IsRequestCacheable(ResponseCachingContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            return base.IsRequestCacheable(context);
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
