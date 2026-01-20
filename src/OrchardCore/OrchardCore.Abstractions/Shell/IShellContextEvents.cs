using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Abstractions.Shell;

public interface IShellContextEvents
{
    Task CreatedAsync(ShellContext context);
}
