using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Commands {
    public class DefaultCommandManager : ICommandManager {
        public DefaultCommandManager() { }

        public void Execute(CommandParameters parameters) {
            Console.WriteLine("EXECUTE!");
        }

        public IEnumerable<CommandDescriptor> GetCommandDescriptors() {
            return Enumerable.Empty<CommandDescriptor>();
        }
    }
}
