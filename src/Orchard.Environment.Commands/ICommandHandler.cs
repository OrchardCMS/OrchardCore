using Orchard.DependencyInjection;

namespace Orchard.Environment.Commands
{
    public interface ICommandHandler : IDependency
    {
        void Execute(CommandContext context);
    }
}