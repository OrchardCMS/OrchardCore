using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
        Task<ServiceProvider> CreateContainerAsync(ShellSettings settings, ShellBlueprint blueprint);
    }
}
