using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Commands
{
    public class DefaultCommandManager : ICommandManager
    {
        private readonly IEnumerable<ICommandHandler> _commandHandlers;
        private readonly CommandHandlerDescriptorBuilder _builder = new CommandHandlerDescriptorBuilder();

        public DefaultCommandManager(IEnumerable<ICommandHandler> commandHandlers)
        {
            _commandHandlers = commandHandlers;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Execute(CommandParameters parameters)
        {
            var matches = MatchCommands(parameters);

            if (matches.Count() == 1)
            {
                var match = matches.Single();
                match.CommandHandler.Execute(match.Context);
            }
            else
            {
                var commandMatch = string.Join(" ", parameters.Arguments.ToArray());
                var commandList = string.Join(",", GetCommandDescriptors().SelectMany(d => d.Names).ToArray());
                if (matches.Any())
                {
                    throw new OrchardCoreException(T("Multiple commands found matching arguments \"{0}\". Commands available: {1}.",
                        commandMatch, commandList));
                }
                throw new OrchardCoreException(T("No command found matching arguments \"{0}\". Commands available: {1}.",
                    commandMatch, commandList));
            }
        }

        public IEnumerable<CommandDescriptor> GetCommandDescriptors()
        {
            return _commandHandlers.SelectMany(x => _builder.Build(x.GetType()).Commands);
        }

        private IEnumerable<Match> MatchCommands(CommandParameters parameters)
        {
            // Command names are matched with as many arguments as possible, in decreasing order
            foreach (var argCount in Enumerable.Range(1, parameters.Arguments.Count()).Reverse())
            {
                int count = argCount;
                var matches = _commandHandlers.SelectMany(h =>
                    MatchCommands(parameters, count, _builder.Build(h.GetType()), h)).ToList();
                if (matches.Any())
                    return matches;
            }

            return Enumerable.Empty<Match>();
        }

        private static IEnumerable<Match> MatchCommands(CommandParameters parameters, int argCount, CommandHandlerDescriptor descriptor, ICommandHandler handler)
        {
            foreach (var commandDescriptor in descriptor.Commands)
            {
                foreach (var name in commandDescriptor.Names)
                {
                    var names = name.Split(' ');
                    if (parameters.Arguments.Take(argCount).SequenceEqual(names, StringComparer.OrdinalIgnoreCase))
                    {
                        yield return new Match
                        {
                            Context = new CommandContext
                            {
                                Arguments = parameters.Arguments.Skip(names.Count()),
                                Command = string.Join(" ", names),
                                CommandDescriptor = commandDescriptor,
                                Input = parameters.Input,
                                Output = parameters.Output,
                                Switches = parameters.Switches,
                            },
                            CommandHandler = handler
                        };
                    }
                }
            }
        }

        private class Match
        {
            public CommandContext Context { get; set; }
            public ICommandHandler CommandHandler { get; set; }
        }
    }
}