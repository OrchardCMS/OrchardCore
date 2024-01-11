using System;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.BackgroundTasks
{
    public class BackgroundTaskEventContext
    {
        public BackgroundTaskEventContext(string name, ShellScope scope)
        {
            Name = name;
            Tenant = scope.ShellContext.Settings.Name;
            Services = scope.ServiceProvider;
        }

        public string Name { get; }
        public string Tenant { get; }
        public IServiceProvider Services { get; }
        public Exception Exception { get; set; }
        public bool HasException => Exception != null;
    }
}
