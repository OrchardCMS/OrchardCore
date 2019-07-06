using Microsoft.AspNetCore.Builder;

namespace OrchardCore.Logging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSerilogTenantNameLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogTenantNameLoggingMiddleware>();
        }
    }
}
