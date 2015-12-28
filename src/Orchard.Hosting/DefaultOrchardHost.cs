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

namespace Orchard.Hosting
{
    public class DefaultOrchardHost : IOrchardHost, IShellDescriptorManagerEventHandler
    {
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
            ILogger<DefaultOrchardHost> logger)
        {
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _logger = logger;
        }

        public void Initialize()
        {
            BuildCurrent();
        }

        /// <summary>
        /// Ensures shells are activated, or re-activated if extensions have changed
        /// </summary>
        IDictionary<string, ShellContext> BuildCurrent()
        {
            if (_shellContexts == null)
            {
                lock (this)
                {
                    if (_shellContexts == null)
                    {
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

        void CreateAndActivateShells()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start creation of shells");
            }

            // Is there any tenant right now?
            var allSettings = _shellSettingsManager.LoadSettings()
                .Where(settings => 
                    settings.State == TenantState.Running || 
                    settings.State == TenantState.Uninitialized || 
                    settings.State == TenantState.Initializing)
                .ToArray();

            // Load all tenants, and activate their shell.
            if (allSettings.Any())
            {
                Parallel.ForEach(allSettings, settings =>
                {
                    try
                    {
                        var context = CreateShellContext(settings);
                        ActivateShell(context);
                    }
                    catch (Exception ex)
                    {
                        if (ex.IsFatal())
                        {
                            throw;
                        }

                        _logger.LogError(string.Format("A tenant could not be started: {0}", settings.Name), ex);
                    }
                });
            }
            // No settings, run the Setup.
            else
            {
                var setupContext = CreateSetupContext();
                ActivateShell(setupContext);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done creating shells");
            }
        }

        /// <summary>
        /// Registers the shell settings in RunningShellTable
        /// </summary>
        private void ActivateShell(ShellContext context)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Activating context for tenant {0}", context.Settings.Name);
            }
            if (_shellContexts.TryAdd(context.Settings.Name, context))
            {
                _runningShellTable.Add(context.Settings);
            }
        }


        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        public ShellContext CreateShellContext(ShellSettings settings)
        {
            if (settings.State == TenantState.Uninitialized)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating shell context for tenant {0} setup", settings.Name);
                }
                return _shellContextFactory.CreateSetupContext(settings);
            }

            _logger.LogDebug("Creating shell context for tenant {0}", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }

        /// <summary>
        /// Creates a transient shell for the default tenant's setup.
        /// </summary>
        private ShellContext CreateSetupContext()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating shell context for root setup.");
            }
            return _shellContextFactory.CreateSetupContext(ShellHelper.BuildDefaultUninitializedShell);
        }

        /// <summary>
        /// A feature is enabled/disabled, the tenant needs to be restarted
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Something changed! ARGH! for tenant {0}", tenant);
            }
        }
    }
}