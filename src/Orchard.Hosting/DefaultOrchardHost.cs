using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.Models;
using Orchard.Hosting.ShellBuilders;

namespace Orchard.Hosting
{
    /// <summary>
    /// All <see cref="ShellContext"/> object are loaded when <see cref="Initialize"/> is called. They can be removed when the
    /// tenant is removed, but are necessary to match an incoming request, even if they are not initialized.
    /// Each <see cref="ShellContext"/> is activated (its service provider is built) on the first request.
    /// </summary>
    public class DefaultOrchardHost : IOrchardHost, IShellDescriptorManagerEventHandler
    {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;

        private readonly static object _syncLock = new object();
        private ConcurrentDictionary<string, ShellContext> _shellContexts;
        private readonly IExtensionManager _extensionManager;

        public DefaultOrchardHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IExtensionManager extensionManager,
            ILogger<DefaultOrchardHost> logger)
        {
            _extensionManager = extensionManager;
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

        public ShellContext GetOrCreateShellContext(ShellSettings settings)
        {
            return _shellContexts.GetOrAdd(settings.Name, tenant =>
            {
                var shellContext = CreateShellContext(settings);
                ActivateShell(shellContext);
                return shellContext;
            });
        }

        public void UpdateShellSettings(ShellSettings settings)
        {
            _shellSettingsManager.SaveSettings(settings);
            ReloadShellContext(settings);
        }

        void CreateAndActivateShells()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start creation of shells");
            }

            // Load all extensions and features so that the controllers are
            // registered in ITypeFeatureProvider and their areas definedin the application
            // conventions.
            _extensionManager.LoadFeatures(_extensionManager.AvailableFeatures());

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
                        GetOrCreateShellContext(settings);
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
        Task IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("A tenant needs to be restarted {0}", tenant);
            }

            if (_shellContexts == null)
            {
                return Task.CompletedTask;
            }

            ShellContext context;
            if (!_shellContexts.TryGetValue(tenant, out context))
            {
                return Task.CompletedTask;
            }

            if (_shellContexts.TryRemove(tenant, out context))
            {
                context.Dispose();
            }

            return Task.CompletedTask;
        }

        public void ReloadShellContext(ShellSettings settings)
        {
            ShellContext context;

            if (_shellContexts.TryRemove(settings.Name, out context))
            {
                _runningShellTable.Remove(settings);
                context.Dispose();
            }
            GetOrCreateShellContext(settings);
        }

        public IEnumerable<ShellContext> ListShellContexts()
        {
            return _shellContexts.Values;
        }
    }
}