using OrchardCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public class OrchardCoreBuilder
    {
        public OrchardCoreBuilder(IServiceCollection services)
        {
            Services = services;
            Startup = new TenantStartupBuilder(this);
        }

        public IServiceCollection Services { get; }
        public TenantStartupBuilder Startup { get; }
        public OrchardCoreBuilder Configure<T>() where T : class, IStartup
        {
            Startup.Builder.Services.AddTransient<IStartup, T>();
            return this;
        }
    }
}
