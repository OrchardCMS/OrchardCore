using System;
using System.Linq;
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

        public OrchardCoreBuilder AddStartups()
        {
            var orders = Startup.Actions.Keys.ToArray();

            foreach (var order in orders)
            {
                var actions = Startup.Actions[order];

                Services.AddTransient<IStartup>(sp => new TenantStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            Startup.Actions.Clear();
            return this;
        }
    }
}
