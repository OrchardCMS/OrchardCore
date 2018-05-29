namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level configuration to serve static files from modules
        /// </summary>
        public static OrchardCoreBuilder UseStaticFiles(this OrchardCoreBuilder builder)
        {
            builder.Startup.Configure((tenant, routes, sp) =>
            {
                tenant.UseStaticFiles();
            });

            return builder;
        }
    }
}
