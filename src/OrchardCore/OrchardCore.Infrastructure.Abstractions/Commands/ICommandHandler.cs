using System.Threading.Tasks;

namespace OrchardCore.Environment.Commands
{
    public interface ICommandHandler
    {
        Task ExecuteAsync(CommandContext context);
    }
}
