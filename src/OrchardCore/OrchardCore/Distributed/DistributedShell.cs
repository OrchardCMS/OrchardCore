using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;

namespace OrchardCore.Distributed
{
    public class DistributedShell : IModularTenantEvents
    {
        private readonly IShellHost _shellHost ;
        private readonly ShellSettings _shellSettings;
        private readonly IMessageBus _messageBus;

        public DistributedShell(IShellHost shellHost, ShellSettings shellSettings, IEnumerable<IMessageBus> _messageBuses)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _messageBus = _messageBuses.LastOrDefault();
        }

        public Task ActivatingAsync() { return Task.CompletedTask; }

        public Task ActivatedAsync()
        {
            if (_messageBus != null)
            {
                return _messageBus.SubscribeAsync("Shell", (channel, message) =>
                {
                    if (message == "Disabled")
                    {
                        _shellSettings.State = TenantState.Disabled;
                        _shellHost.UpdateShellSettingsAsync(_shellSettings).GetAwaiter().GetResult();
                    }

                    if (message == "Terminated" || message == "Disabled")
                    {
                        _shellHost.ReloadShellContextAsync(_shellSettings).GetAwaiter().GetResult();
                    }
                });
            }

            return Task.CompletedTask;
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }

        public Task TerminatedAsync()
        {
            if (_messageBus != null)
            {
                if (_shellSettings.State == TenantState.Disabled)
                {
                    return _messageBus.PublishAsync("Shell", "Disabled");
                }

                return _messageBus.PublishAsync("Shell", "Terminated");
            }

            return Task.CompletedTask;
        }
    }
}
