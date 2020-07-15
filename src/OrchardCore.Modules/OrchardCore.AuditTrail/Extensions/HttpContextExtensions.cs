using Microsoft.AspNetCore.Http;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetCurrentUserName(this IHttpContextAccessor hca) =>
            hca.HttpContext.User?.Identity?.Name;
    }
}
