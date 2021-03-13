using Microsoft.AspNetCore.Http;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetCurrentUserName(this IHttpContextAccessor httpContextAccessor) =>
            httpContextAccessor.HttpContext.User?.Identity?.Name;
    }
}
