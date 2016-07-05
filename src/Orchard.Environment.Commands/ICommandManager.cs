using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Commands
{
    public interface ICommandManager
    {
        Task ExecuteAsync(CommandParameters parameters);
        IEnumerable<CommandDescriptor> GetCommandDescriptors();
    }
}