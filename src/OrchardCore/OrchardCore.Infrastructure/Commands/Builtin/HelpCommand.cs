using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Environment.Commands.Builtin
{
    public class HelpCommand : DefaultCommandHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandHandlerDescriptorBuilder _builder = new();

        public HelpCommand(IServiceProvider serviceProvider,
            IStringLocalizer<HelpCommand> localizer) : base(localizer)
        {
            _serviceProvider = serviceProvider;
        }

        [CommandName("help commands")]
        [CommandHelp("help commands", "\tDisplay help text for all available commands")]
        public async Task AllCommandsAsync()
        {
            await Context.Output.WriteLineAsync(S["List of available commands:"]);
            await Context.Output.WriteLineAsync("---------------------------");
            await Context.Output.WriteLineAsync();

            var descriptors = GetCommandDescriptors().OrderBy(d => d.Names.First());

            foreach (var descriptor in descriptors)
            {
                await Context.Output.WriteLineAsync(GetHelpText(descriptor));
                await Context.Output.WriteLineAsync();
            }
        }

        [CommandName("help")]
        [CommandHelp("help <command>", "\tDisplay help text for <command>")]
        public async Task SingleCommandAsync(string[] commandNameStrings)
        {
            var command = String.Join(" ", commandNameStrings);
            var descriptors = GetCommandDescriptors()
                .Where(t => t.Names.Any(x => x.StartsWith(command, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(d => d.Names);

            if (!descriptors.Any())
            {
                await Context.Output.WriteLineAsync(S["Command {0} doesn't exist", command]);
            }
            else
            {
                foreach (var descriptor in descriptors)
                {
                    await Context.Output.WriteLineAsync(GetHelpText(descriptor));
                    await Context.Output.WriteLineAsync();
                }
            }
        }
        private IEnumerable<CommandDescriptor> GetCommandDescriptors()
        {
            var commandhandlers = _serviceProvider.GetServices<ICommandHandler>();
            return commandhandlers.SelectMany(x => _builder.Build(x.GetType()).Commands);
        }

        private LocalizedString GetHelpText(CommandDescriptor descriptor)
        {
            if (String.IsNullOrEmpty(descriptor.HelpText))
            {
                return S["{0}.{1}: no help text", descriptor.MethodInfo.DeclaringType?.FullName, descriptor.MethodInfo.Name];
            }

            return S[descriptor.HelpText];
        }
    }
}
