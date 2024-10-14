using Microsoft.AspNetCore.Builder;

namespace OrchardCore.Logging;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSerilogTenantNameLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SerilogTenantNameLoggingMiddleware>();
    }
}
