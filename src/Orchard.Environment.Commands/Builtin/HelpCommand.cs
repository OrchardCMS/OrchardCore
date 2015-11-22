using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Orchard.Environment.Commands.Builtin
{
    public class HelpCommand : DefaultOrchardCommandHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandHandlerDescriptorBuilder _builder = new CommandHandlerDescriptorBuilder();

        public HelpCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [CommandName("help commands")]
        [CommandHelp("help commands\r\n\tDisplay help text for all available commands")]
        public void AllCommands()
        {
            Context.Output.WriteLine(T("List of available commands:"));
            Context.Output.WriteLine(T("---------------------------"));
            Context.Output.WriteLine("");

            var descriptors = GetCommandDescriptors().OrderBy(d => d.Names.First());

            foreach (var descriptor in descriptors)
            {
                Context.Output.WriteLine(GetHelpText(descriptor));
                Context.Output.WriteLine("");
            }
        }


        [CommandName("help")]
        [CommandHelp("help <command>\r\n\tDisplay help text for <command>")]
        public void SingleCommand(string[] commandNameStrings)
        {
            string command = string.Join(" ", commandNameStrings);
            var descriptors = GetCommandDescriptors()
                .Where(t => t.Names.Any(x => x.StartsWith(command, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(d => d.Names);

            if (!descriptors.Any())
            {
                Context.Output.WriteLine(T($"Command {command} doesn't exist"));
            }
            else
            {
                foreach (var descriptor in descriptors)
                {
                    Context.Output.WriteLine(GetHelpText(descriptor));
                    Context.Output.WriteLine("");
                }
            }
        }
        private IEnumerable<CommandDescriptor> GetCommandDescriptors()
        {
            var commandhandlers = _serviceProvider.GetService<IEnumerable<ICommandHandler>>();
            return commandhandlers.SelectMany(x => _builder.Build(x.GetType()).Commands);
        }

        private LocalizedString GetHelpText(CommandDescriptor descriptor)
        {
            if (string.IsNullOrEmpty(descriptor.HelpText))
            {
                return T($"{descriptor.MethodInfo.DeclaringType?.FullName}.{descriptor.MethodInfo.Name}: no help text");
            }

            return T(descriptor.HelpText);
        }
    }
}