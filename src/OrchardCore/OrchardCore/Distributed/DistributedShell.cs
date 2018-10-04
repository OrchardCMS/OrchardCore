using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Distributed.Messaging;
using OrchardCore.Environment.Shell;
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
                _messageBus.Subscribe("Shell", (channel, message) =>
                {
                    if (message == "Terminated")
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
            _messageBus?.Publish("Shell", "Terminated");
            return Task.CompletedTask;
        }
    }
}
