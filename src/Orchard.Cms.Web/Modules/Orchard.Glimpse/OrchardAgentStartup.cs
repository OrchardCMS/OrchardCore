using System;
using System.Diagnostics;
using Glimpse.Agent.Configuration;
using Glimpse.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Glimpse.Inspectors;

namespace Orchard.Glimpse
{
    public class OrchardAgentStartup : IAgentStartup
    {
        public OrchardAgentStartup(IRequestIgnorerManager requestIgnorerManager)
        {
            RequestIgnorerManager = requestIgnorerManager;
        }

        private IRequestIgnorerManager RequestIgnorerManager { get; }

        public void Run(IStartupOptions options)
        {
            var appServices = options.ApplicationServices;

            var listenerSubscription = DiagnosticListener.AllListeners.Subscribe(listener =>
            {
                listener.SubscribeWithAdapter(appServices.GetRequiredService<OrchardWebDiagnosticsInspector>(), IsEnabled);
            });
        }

        private bool IsEnabled(string topic)
        {
            return !RequestIgnorerManager.ShouldIgnore();
        }
    }
}
