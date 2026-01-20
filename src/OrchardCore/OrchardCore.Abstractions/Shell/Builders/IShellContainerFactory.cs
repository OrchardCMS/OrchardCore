using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders;

public interface IShellContainerFactory
{
    Task<IServiceProvider> CreateContainerAsync(ShellSettings settings, ShellBlueprint blueprint);
}
