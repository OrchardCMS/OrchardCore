using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Commands.Builtin;
using OrchardCore.Environment.Commands.Parameters;

namespace OrchardCore.Environment.Commands
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