using System.Threading.Tasks;

namespace Orchard.Environment.Commands
{
    public interface ICommandHandler
    {
        Task ExecuteAsync(CommandContext context);
    }
}