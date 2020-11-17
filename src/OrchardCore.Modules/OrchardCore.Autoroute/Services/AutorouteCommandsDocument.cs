using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteCommandsDocument : Document
    {
        public const int MaxCommandsCount = 1_000;

        public List<AutorouteCommand> Commands { get; set; } = new List<AutorouteCommand>();

        public void AddCommand(string name, IEnumerable<AutorouteEntry> entries)
        {
            // We don't remove an obsolete command as we need to hold its 'Id',
            // but we can remove its obsolete autoroute entries.
            foreach (var entry in entries)
            {
                foreach (var command in Commands)
                {
                    if (command.Name == name)
                    {
                        command.Entries.RemoveAll(e =>
                            e.ContentItemId == entry.ContentItemId &&
                            e.ContainedContentItemId == entry.ContainedContentItemId);

                        if (command.Entries.Count == 0)
                        {
                            command.Name = AutorouteCommand.Obsolete;
                        }
                    }
                }
            }

            // Add the new command to process.
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
        public static string Obsolete = String.Empty;

        public string Id { get; set; }
        public string Name { get; set; }
        public List<AutorouteEntry> Entries { get; set; }
    }
}
