using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// All <see cref="ShellContext"/> object are loaded when <see cref="Initialize"/> is called. They can be removed when the
    /// tenant is removed, but are necessary to match an incoming request, even if they are not initialized.
    /// Each <see cref="ShellContext"/> is activated (its service provider is built) on the first request.
    /// </summary>
    public class ShellHost : IShellHost, IShellDescriptorManagerEventHandler
    {
        private static EventId TenantNotStarted = new EventId(0);

        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;

        private readonly static object _syncLock = new object();
        private ConcurrentDictionary<string, Lazy<ShellContext>> _shellContexts;
        private readonly IExtensionManager _extensionManager;

        public ShellHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IExtensionManager extensionManager,
            ILogger<ShellHost> logger)
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
        IDictionary<string, Lazy<ShellContext>> BuildCurrent()
        {
            if (_shellContexts == null)
            {
                lock (this)
                {
                    if (_shellContexts == null)
                    {
                        _shellContexts = new ConcurrentDictionary<string, Lazy<ShellContext>>();
                        CreateAndRegisterShellsAsync().Wait();
                    }
                }
            }

            return _shellContexts;
        }

        public ShellContext GetOrCreateShellContext(ShellSettings settings)
        {
            var shell = _shellContexts.GetOrAdd(settings.Name, new Lazy<ShellContext>(() =>
            {
                var shellContext = CreateShellContextAsync(settings).Result;
                RegisterShell(shellContext);
                return shellContext;
            })).Value;

            if (shell.Released)
            {
                _shellContexts.TryRemove(settings.Name, out var context);
                return GetOrCreateShellContext(settings);
            }

            return shell;
        }

        public void UpdateShellSettings(ShellSettings settings)
        {
            _shellSettingsManager.SaveSettings(settings);
            ReloadShellContext(settings);
        }

        async Task CreateAndRegisterShellsAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start creation of shells");
            }

            // Load all extensions and features so that the controllers are
            // registered in ITypeFeatureProvider and their areas defined in the application
            // conventions.
            var features = _extensionManager.LoadFeaturesAsync();

            // Is there any tenant right now?
            var allSettings = _shellSettingsManager.LoadSettings().Where(CanCreateShell).ToArray();

            features.Wait();

            // No settings, run the Setup.
            if (allSettings.Length == 0)
            {
                var setupContext = await CreateSetupContextAsync();
                AddAndRegisterShell(setupContext);
            }
            else
            {
                // Load all tenants, and activate their shell.
                Parallel.ForEach(allSettings, settings =>
                {
                    try
                    {
                        GetOrCreateShellContext(settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(TenantNotStarted, ex, $"A tenant could not be started: {settings.Name}");

                        if (ex.IsFatal())
                        {
                            throw;
                        }
                    }
                });
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done creating shells");
            }
        }

        /// <summary>
        /// Whether or not a shell can be activated and added to the running shells.
        /// </summary>
        private bool CanRegisterShell(ShellContext context)
        {
            if (!CanRegisterShell(context.Settings))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Skipping shell context registration for tenant {0}", context.Settings.Name);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers the shell settings in RunningShellTable
        /// </summary>
        private void RegisterShell(ShellContext context)
        {
            if (!CanRegisterShell(context))
            {
                return;
            }

            RegisterSettings(context.Settings);
        }

        /// <summary>
        /// Adds the shell and registers its settings in RunningShellTable
        /// </summary>
        private void AddAndRegisterShell(ShellContext context)
        {
            if (!CanRegisterShell(context))
            {
                return;
            }

            if (_shellContexts.TryAdd(context.Settings.Name, new Lazy<ShellContext>(() => context)))
            {
                RegisterSettings(context.Settings);
            }
        }

        /// <summary>
        /// Creates a shell context based on shell settings
        /// </summary>
        public Task<ShellContext> CreateShellContextAsync(ShellSettings settings)
        {
            if (settings.State == TenantState.Uninitialized)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating shell context for tenant {0} setup", settings.Name);
                }

                return _shellContextFactory.CreateSetupContextAsync(settings);
            }
            else if (settings.State == TenantState.Disabled)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating disabled shell context for tenant {0} setup", settings.Name);
                }

                return Task.FromResult(new ShellContext { Settings = settings });
            }
            else if(settings.State == TenantState.Running || settings.State == TenantState.Initializing)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating shell context for tenant {0}", settings.Name);
                }

                return _shellContextFactory.CreateShellContextAsync(settings);
            }
            else
            {
                throw new InvalidOperationException("Unexpected shell state for " + settings.Name);
            }
        }

        /// <summary>
        /// Creates a transient shell for the default tenant's setup.
        /// </summary>
        private Task<ShellContext> CreateSetupContextAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating shell context for root setup.");
            }

            return _shellContextFactory.CreateSetupContextAsync(ShellHelper.BuildDefaultUninitializedShell);
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

            if (_shellContexts.TryRemove(tenant, out var context))
            {
                context.Value.Release();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Marks the specific tenant as released, such that a new shell is created for subsequent requests,
        /// while existing requests get flushed.
        /// </summary>
        /// <param name="settings"></param>
        public void ReloadShellContext(ShellSettings settings)
        {
            if (_shellContexts.TryRemove(settings.Name, out var context))
            {
                _runningShellTable.Remove(settings);
                context.Value.Release();
            }

            GetOrCreateShellContext(settings);
        }

        public IEnumerable<ShellContext> ListShellContexts()
        {
            // Prevent to acquire all the locks at once.
            return _shellContexts?.Select(kv => kv.Value.Value);
        }

        /// <summary>
        /// Whether or not a shell can be added to the list of available shells.
        /// </summary>
        private bool CanCreateShell(ShellSettings shellSettings)
        {
            return
                shellSettings.State == TenantState.Running ||
                shellSettings.State == TenantState.Uninitialized ||
                shellSettings.State == TenantState.Initializing ||
                shellSettings.State == TenantState.Disabled;
        }

        /// <summary>
        /// Whether or not a shell can be activated and added to the running shells.
        /// </summary>
        private bool CanRegisterShell(ShellSettings shellSettings)
        {
            return
                shellSettings.State == TenantState.Running ||
                shellSettings.State == TenantState.Uninitialized ||
                shellSettings.State == TenantState.Initializing;
        }

        /// <summary>
        /// Registers the shell settings in RunningShellTable
        /// </summary>
        private void RegisterSettings(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Registering shell context for tenant {0}", settings.Name);
            }

            _runningShellTable.Add(settings);
        }
    }
}
