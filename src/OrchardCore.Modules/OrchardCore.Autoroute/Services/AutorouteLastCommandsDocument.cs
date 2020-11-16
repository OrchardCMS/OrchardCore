using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteLastCommandsDocument : Document
    {
        public const int MaxCommandsCount = 100;

        public List<AutorouteCommand> Commands { get; set; } = new List<AutorouteCommand>();

        public void AddCommand(string name, IEnumerable<AutorouteEntry> entries)
        {
            Commands.Add(new AutorouteCommand()
            {
                Name = name,
                Id = Identifier,
                Entries = new List<AutorouteEntry>(entries)
            });

            // Limit the commands list length.
            if (Commands.Count > MaxCommandsCount)
            {
                Commands = Commands.Skip(Commands.Count - MaxCommandsCount).ToList();
            }
        }

        public bool TryGetLastCommands(string lastCommandId, out IEnumerable<AutorouteCommand> commands)
        {
            var index = Commands.FindLastIndex(x => x.Id == lastCommandId);
            if (index != -1)
            {
                if (Commands.Count == index + 1)
                {
                    // Nothing to return, we are up to date.
                    commands = Enumerable.Empty<AutorouteCommand>();
                    return true;
                }

                // Return the last commands that are not yet processed.
                commands = Commands.Skip(index + 1);
                return true;
            }

            if (Commands.Count >= MaxCommandsCount)
            {
                // The last command was not found and the max count was reached,
                // so we may have missed some commands.
                commands = null;
                return false;
            }

            // Otherwise return the full list.
            commands = Commands;
            return true;
        }
    }

    public class AutorouteCommand
    {
        public const string AddEntries = nameof(AddEntries);
        public const string RemoveEntries = nameof(RemoveEntries);

        public string Id { get; set; }
        public string Name { get; set; }
        public List<AutorouteEntry> Entries { get; set; }
    }
}
