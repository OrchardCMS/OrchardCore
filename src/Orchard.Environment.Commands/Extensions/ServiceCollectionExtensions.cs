using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            services.AddScoped<ICommandManager, DefaultCommandManager>();

            return services;
        }
    }
}