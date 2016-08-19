using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands.Builtin;
using Orchard.Environment.Commands.Parameters;

namespace Orchard.Environment.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, HelpCommand>();

            services.AddScoped<ICommandParametersParser, CommandParametersParser>();
            services.AddScoped<ICommandParser, CommandParser>();

            return services;
        }
    }
}