using Microsoft.AspNetCore.Builder;

namespace Orchard.DeferredTasks
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddDeferredTasks(this IApplicationBuilder app)
        {
            // TODO: Order to be the late in the return pipeline
            app.UseMiddleware<DeferredTaskMiddleware>();

            return app;
        }
    }
}
