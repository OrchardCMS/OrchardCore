using System.Collections.Generic;

namespace OrchardCore.Environment.Commands
{
    public class CommandHandlerDescriptor
    {
        public IEnumerable<CommandDescriptor> Commands { get; set; }
    }
}
