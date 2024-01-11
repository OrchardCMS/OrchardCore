using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Commands
{
    public interface ICommandManager
    {
        Task ExecuteAsync(CommandParameters parameters);
        IEnumerable<CommandDescriptor> GetCommandDescriptors();
    }
}
