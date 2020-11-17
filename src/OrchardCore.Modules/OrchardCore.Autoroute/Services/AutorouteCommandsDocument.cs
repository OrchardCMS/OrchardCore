using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteCommandsDocument : Document
    {
        public const int MaxCommandsCount = 1_000;

        public List<object> Commands { get; set; } = new List<object>();

        public void AddCommand(string name, IEnumerable<AutorouteEntry> entries)
        {
            // Remove obsolete autoroute entries.
            foreach (var entry in entries)
            {
                foreach (var commandObj in Commands)
                {
                    if (commandObj is AutorouteCommand command && command.Name == name)
                    {
                        command.Entries.RemoveAll(e =>
                            e.ContentItemId == entry.ContentItemId &&
                            e.ContainedContentItemId == entry.ContainedContentItemId);

                        if (command.Entries.Count == 0)
                        {
                            command.Entries = null;
                        }
                    }
                }
            }

            // Remove obsolete commands but still keep their 'Id' string for history.
            while (true)
            {
                var index = Commands.FindLastIndex(o => o is AutorouteCommand c && c.Entries == null);
                if (index == -1)
                {
                    break;
                }

                Commands[index] = ((AutorouteCommand)Commands[index]).Id;
            }

            // Add the new command to process.
            Commands.Add(new AutorouteCommand()
            {
                Name = name,
                Id = Identifier,
                Entries = new List<AutorouteEntry>(entries)
            });

            // Limit the commands list length, the ids strings having a lower weigth.
            var idsCount = Commands.OfType<string>().Count();
            var count = Commands.Count - (9 * idsCount / 10);
            if (count > MaxCommandsCount)
            {
                Commands = Commands.Skip(count - MaxCommandsCount).ToList();
            }
        }

        public bool TryGetLastCommands(string lastCommandId, out IEnumerable<AutorouteCommand> commands)
        {
            var index = Commands.FindLastIndex(o =>
                (o is AutorouteCommand c && c.Id == lastCommandId) || o is string id && id == lastCommandId);
            if (index != -1)
            {
                if (Commands.Count == index + 1)
                {
                    // Nothing to return, we are up to date.
                    commands = Enumerable.Empty<AutorouteCommand>();
                    return true;
                }

                // Return the last commands that are not yet processed.
                commands = Commands.Skip(index + 1).OfType<AutorouteCommand>();
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
            commands = Commands.OfType<AutorouteCommand>();
            return true;
        }
    }

    public class AutorouteCommand
    {
        public const string AddEntries = "A";
        public const string RemoveEntries = "R";

        public string Id { get; set; }
        public string Name { get; set; }
        public List<AutorouteEntry> Entries { get; set; }
    }
}
