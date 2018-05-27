using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules
{
    public class OrchardCoreBuilder
    {
        public OrchardCoreBuilder(IServiceCollection services)
        {
            Services = services;
            Startup = new OrchardCoreStartupBuilder(this);
        }

        public IServiceCollection Services { get; }
        public OrchardCoreStartupBuilder Startup { get; }

        public OrchardCoreBuilder AddStartups()
        {
            var orders = Startup.Actions.Keys.ToArray();

            foreach (var order in orders)
            {
                var actions = Startup.Actions[order];

                Services.AddTransient<IStartup>(sp => new OrchardCoreStartup(
                    sp.GetRequiredService<IServiceProvider>(), actions, order));
            }

            Startup.Actions.Clear();
            return this;
        }
    }
}
