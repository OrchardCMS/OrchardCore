using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.Models;
using Orchard.Hosting.ShellBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Hosting {
    public class DefaultOrchardHost : IOrchardHost, IShellDescriptorManagerEventHandler {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;

        private readonly static object _syncLock = new object();
        private ConcurrentDictionary<string, ShellContext> _shellContexts;

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager, 
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            ILoggerFactory loggerFactory) {
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _logger = loggerFactory.CreateLogger<DefaultOrchardHost>();
        }

        void IOrchardHost.Initialize() {
            BuildCurrent();
        }

        /// <summary>
        /// Ensures shells are activated, or re-activated if extensions have changed
        /// </summary>
        IDictionary<string, ShellContext> BuildCurrent() {
            if (_shellContexts == null) {
                lock (this) {
                    if (_shellContexts == null) {
                        _shellContexts = new ConcurrentDictionary<string, ShellContext>();
                        CreateAndActivateShells();
                    }
                }
            }

            return _shellContexts;
        }

        public ShellContext GetShellContext(ShellSettings settings)
        {
            return _shellContexts[settings.Name];
        }

        public void UpdateShellSettings(ShellSettings settings)
        {
            ShellContext context;

            _shellSettingsManager.SaveSettings(settings);
            _runningShellTable.Update(settings);
            _shellContexts.TryRemove(settings.Name, out context);
            context = CreateShellContext(settings);
            ActivateShell(context);
        }

        void CreateAndActivateShells() {
            _logger.LogInformation("Start creation of shells");

            // Is there any tenant right now?
            var allSettings = _shellSettingsManager.LoadSettings()
                .Where(settings => settings.State == TenantState.Running || settings.State == TenantState.Uninitialized || settings.State == TenantState.Initializing)
                .ToArray();

            // Load all tenants, and activate their shell.
            if (allSettings.Any()) {
                Parallel.ForEach(allSettings, settings => {
                    try {
                        var context = CreateShellContext(settings);
                        ActivateShell(context);
                    }
                    catch (Exception ex) {
                        if (ex.IsFatal()) {
                            throw;
                        }
                        _logger.LogError(string.Format("A tenant could not be started: {0}", settings.Name), ex);
                    }
                });
            }
            // No settings, run the Setup.
            else {
                var setupContext = CreateSetupContext();
                ActivateShell(setupContext);
            }

            _logger.LogInformation("Done creating shells");
        }

        /// <summary>
        /// Registers the shell settings in RunningShellTable
        /// </summary>
        private void ActivateShell(ShellContext context) {
            _logger.LogDebug("Activating context for tenant {0}", context.Settings.Name);

            lock (_shellContexts)
            {
                _shellContexts[context.Settings.Name] = context;
            }

            _runningShellTable.Add(context.Settings);
        }

        /// <summary>
        /// Creates a transient shell for the default tenant's setup.
        /// </summary>
        private ShellContext CreateSetupContext() {
            _logger.LogDebug("Creating shell context for root setup.");
            return _shellContextFactory.CreateSetupContext(ShellHelper.BuildDefaultUninitializedShell);
        }

        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        public ShellContext CreateShellContext(ShellSettings settings) {
            if (settings.State == TenantState.Uninitialized) {
                _logger.LogDebug("Creating shell context for tenant {0} setup", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            _logger.LogDebug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        public void ActivateShell(ShellSettings settings) {
            _logger.LogDebug("Activating shell: {0}", settings.Name);

            // look for the associated shell context
            ShellContext shellContext = null;
            _shellContexts.TryGetValue(settings.Name, out shellContext);
            
            if (shellContext == null && settings.State == TenantState.Disabled) {
                return;
            }

            // is this is a new tenant ? or is it a tenant waiting for setup ?
            if (shellContext == null || settings.State == TenantState.Uninitialized) {
                // create the Shell
                var context = CreateShellContext(settings);

                // activate the Shell
                ActivateShell(context);
            }
            // terminate the shell if the tenant was disabled
            else if (settings.State == TenantState.Disabled) {
                shellContext.Shell.Terminate();
                _runningShellTable.Remove(settings);

                if(_shellContexts.TryRemove(settings.Name, out shellContext))
                {
                    shellContext.Dispose();
                }
            }
            // reload the shell as its settings have changed
            else {
                // dispose previous context
                shellContext.Shell.Terminate();

                var context = _shellContextFactory.CreateShellContext(settings);

                if (_shellContexts.TryRemove(settings.Name, out shellContext))
                {
                    shellContext.Dispose();
                    context.Shell.Activate();
                    _runningShellTable.Update(settings);
                }                
            }
        }

        /// <summary>
        /// A feature is enabled/disabled, the tenant needs to be restarted
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant) {
            _logger.LogDebug("Something changed! ARGH! for tenant {0}", tenant);
        }
    }
}