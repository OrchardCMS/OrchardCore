using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Environment.Commands
{
    public interface ICommandManager : IDependency
    {
        void Execute(CommandParameters parameters);
        IEnumerable<CommandDescriptor> GetCommandDescriptors();
    }
}