using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        [Obsolete("This method will be removed in a future version, use CreateContainerAsync instead.", false)]
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
        Task<IServiceProvider> CreateContainerAsync(ShellSettings settings, ShellBlueprint blueprint);
    }
}
