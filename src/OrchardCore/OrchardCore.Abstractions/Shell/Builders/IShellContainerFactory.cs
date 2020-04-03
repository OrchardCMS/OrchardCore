using OrchardCore.Environment.Shell.Builders.Models;
using System;

namespace OrchardCore.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }
}