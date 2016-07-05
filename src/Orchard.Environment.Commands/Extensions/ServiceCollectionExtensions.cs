using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands.Builtin;

namespace Orchard.Environment.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, HelpCommand>();

            return services;
        }
    }
}