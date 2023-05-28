using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
        Task<IServiceProvider> CreateContainerAsync(ShellSettings settings, ShellBlueprint blueprint);
    }
}
