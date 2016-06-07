using System;
using Orchard.Environment.Shell;

namespace Orchard.Processing
{
    public class DeferredTaskContext
    {
        public DeferredTaskContext(IServiceProvider serviceProvider, ShellSettings shellSettings)
        {
            ServiceProvider = serviceProvider;
            ShellSettings = shellSettings;
        }

        public IServiceProvider ServiceProvider { get; }
        public ShellSettings ShellSettings { get; }
    }
}
