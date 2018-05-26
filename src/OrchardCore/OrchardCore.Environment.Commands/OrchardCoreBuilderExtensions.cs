using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Commands.Builtin;
using OrchardCore.Environment.Commands.Parameters;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Commands
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host level services to provide CLI commands.
        /// </summary>
        public static OrchardCoreBuilder AddCommands(this OrchardCoreBuilder builder)
        {
            AddCommandsHostServices(builder.Services);
            return builder;
        }

        public static void AddCommandsHostServices(IServiceCollection services)
        {
            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, HelpCommand>();

            services.AddScoped<ICommandParametersParser, CommandParametersParser>();
            services.AddScoped<ICommandParser, CommandParser>();
        }
    }
}