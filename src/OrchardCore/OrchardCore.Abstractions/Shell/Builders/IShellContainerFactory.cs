using System;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }
}
