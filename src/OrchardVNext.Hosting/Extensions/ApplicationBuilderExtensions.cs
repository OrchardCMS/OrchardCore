using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Hosting {
    public static class ApplicationBuilderExtensions {
        public static void InitializeHost([NotNull] this IApplicationBuilder builder) {
            var host = builder.ApplicationServices.GetRequiredService<IOrchardHost>();
            host.Initialize();
        }
    }
}