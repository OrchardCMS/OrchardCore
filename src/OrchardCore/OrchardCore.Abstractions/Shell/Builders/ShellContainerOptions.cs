using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

internal class ShellContainerOptions
{
    /// <summary>
    /// Delegates to be invoked asynchronously after a tenant container is created.
    /// </summary>
    public List<Func<IServiceProvider, ValueTask>> Initializers { get; } = new();
}
