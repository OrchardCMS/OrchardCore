using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        Task<IServiceProvider> CreateContainerAsync(ShellSettings settings, ShellBlueprint blueprint);
    }

    public static class ShellContainerFactoryExtensions
    {
        [Obsolete("This method will be removed in a future version, use CreateContainerAsync instead.", false)]
        public static IServiceProvider CreateContainer(this IShellContainerFactory factory, ShellSettings settings, ShellBlueprint blueprint)
            => factory.CreateContainerAsync(settings, blueprint).GetAwaiter().GetResult();
    }
}
