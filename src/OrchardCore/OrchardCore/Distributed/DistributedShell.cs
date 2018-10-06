using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task ActivatedAsync()
        {
            if (_messageBus != null)
            {
                await _messageBus.SubscribeAsync("Shell", (channel, message) =>
                {
                    if (message == "Terminated")
                    {
                        _shellHost.ReloadShellContextAsync(_shellSettings).GetAwaiter().GetResult();
                    }
                });
            }
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }

        public Task TerminatedAsync()
        {
            if (_messageBus != null)
            {
                return _messageBus.PublishAsync("Shell", "Terminated");
            }

            return Task.CompletedTask;
        }
    }
}
