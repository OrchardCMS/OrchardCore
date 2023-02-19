using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

internal class ShellContainerOptions
{
    /// <summary>
    /// Initialize delegates to be executed asynchronously on tenant container creation.
    /// </summary>
    public readonly List<Func<IServiceProvider, ValueTask>> Initializers = new();
}
