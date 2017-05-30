using Microsoft.AspNetCore.Builder;

namespace Orchard.DeferredTasks
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddDeferredTasks(this IApplicationBuilder app)
        {
            app.UseMiddleware<DeferredTaskMiddleware>();

            return app;
        }
    }
}
