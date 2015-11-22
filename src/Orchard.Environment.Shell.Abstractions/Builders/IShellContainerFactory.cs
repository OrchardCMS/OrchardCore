using Orchard.Environment.Shell.Builders.Models;
using System;

namespace Orchard.Environment.Shell.Builders
{
    public interface IShellContainerFactory
    {
        IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }
}