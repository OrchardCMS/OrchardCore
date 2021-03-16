using OrchardCore.Environment.Commands;
using OrchardCore.Environment.Commands.Builtin;
using OrchardCore.Environment.Commands.Parameters;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds host level services to provide CLI commands.
        /// </summary>
        public static OrchardCoreBuilder AddCommands(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, HelpCommand>();

            services.AddScoped<ICommandParametersParser, CommandParametersParser>();
            services.AddScoped<ICommandParser, CommandParser>();

            return builder;
        }
    }
}
