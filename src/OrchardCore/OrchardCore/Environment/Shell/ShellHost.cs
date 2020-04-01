using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// All <see cref="ShellContext"/> object are loaded when <see cref="InitializeAsync"/> is called. They can be removed when the
    /// tenant is removed, but are necessary to match an incoming request, even if they are not initialized.
    /// Each <see cref="ShellContext"/> is activated (its service provider is built) on the first request.
    /// </summary>
    public class ShellHost : IShellHost, IDisposable
    {
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;

        private bool _initialized;
        private ConcurrentDictionary<string, ShellContext> _shellContexts = new ConcurrentDictionary<string, ShellContext>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        private SemaphoreSlim _initializingSemaphore = new SemaphoreSlim(1);

        public ShellHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IExtensionManager extensionManager,
            ILogger<ShellHost> logger)
        {
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _extensionManager = extensionManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            if (!_initialized)
            {
                try
                {
                    // Prevent concurrent requests from creating all shells multiple times
                    await _initializingSemaphore.WaitAsync();

                    if (!_initialized)
                    {
                        await PreCreateAndRegisterShellsAsync();
                    }
                }
                finally
                {
                    _initialized = true;
                    _initializingSemaphore.Release();
                }
            }
        }

        public async Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings)
        {
            ShellContext shell = null;

            while (shell == null)
            {
                if (!_shellContexts.TryGetValue(settings.Name, out shell))
                {
                    var semaphore = _shellSemaphores.GetOrAdd(settings.Name, (name) => new SemaphoreSlim(1));

                    await semaphore.WaitAsync();

                    try
                    {
                        if (!_shellContexts.TryGetValue(settings.Name, out shell))
                        {
                            shell = await CreateShellContextAsync(settings);
                            AddAndRegisterShell(shell);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        _shellSemaphores.TryRemove(settings.Name, out semaphore);
                    }
                }

                if (shell.Released)
                {
                    // If the context is released, it is removed from the dictionary so that
                    // a new call on 'GetOrCreateShellContextAsync' will recreate a new shell context.
                    _shellContexts.TryRemove(settings.Name, out var value);
                    shell = null;
                }
            }

            return shell;
        }

        public async Task<ShellScope> GetScopeAsync(ShellSettings settings)
        {
            ShellScope scope = null;
            ShellContext shellContext = null;

            while (scope == null)
            {
                if (!_shellContexts.TryGetValue(settings.Name, out shellContext))
                {
                    shellContext = await GetOrCreateShellContextAsync(settings);
                }

                // We create a scope before checking if the shell has been released.
                scope = shellContext.CreateScope();

                // If CreateScope() returned null, the shell is released. We then remove it and
                // retry with the hope to get one that won't be released before we create a scope.
                if (scope == null)
                {
                    // If the context is released, it is removed from the dictionary so that
                    // a new call on 'GetScope' will recreate a new shell context.
                    _shellContexts.TryRemove(settings.Name, out var value);
                }
            }

            return scope;
        }

        public async Task UpdateShellSettingsAsync(ShellSettings settings)
        {
            await _shellSettingsManager.SaveSettingsAsync(settings);
            await ReloadShellContextAsync(settings);
        }

        private async Task PreCreateAndRegisterShellsAsync()
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
            var allSettings = (await _shellSettingsManager.LoadSettingsAsync()).Where(CanCreateShell).ToArray();
            var defaultSettings = allSettings.FirstOrDefault(s => s.Name == ShellHelper.DefaultShellName);
            var otherSettings = allSettings.Except(new[] { defaultSettings }).ToArray();

            await features;

            // The 'Default' tenant is not running, run the Setup.
            if (defaultSettings?.State != TenantState.Running)
            {
                var setupContext = await CreateSetupContextAsync(defaultSettings);
                AddAndRegisterShell(setupContext);
                allSettings = otherSettings;
            }

            if (allSettings.Length > 0)
            {
                // Pre-create and register all tenant shells.
                foreach (var settings in allSettings)
                {
                    if (settings.Name == ShellHelper.DefaultShellName)
                    {
                        await GetOrCreateShellContextAsync(settings);
                        continue;
                    }

                    AddAndRegisterShell(new ShellContext.PlaceHolder { Settings = settings });
                };
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done pre-creating and registering shells");
            }
        }

        /// <summary>
        /// Adds the shell and registers its settings in RunningShellTable
        /// </summary>
        private void AddAndRegisterShell(ShellContext context)
        {
            if (_shellContexts.TryAdd(context.Settings.Name, context) && CanRegisterShell(context))
            {
                RegisterShellSettings(context.Settings);
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
                    _logger.LogDebug("Skipping shell context registration for tenant '{TenantName}'", context.Settings.Name);
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers the shell settings in RunningShellTable
        /// </summary>
        private void RegisterShellSettings(ShellSettings settings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Registering shell context for tenant '{TenantName}'", settings.Name);
            }

            _runningShellTable.Add(settings);
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
                    _logger.LogDebug("Creating shell context for tenant '{TenantName}' setup", settings.Name);
                }

                return _shellContextFactory.CreateSetupContextAsync(settings);
            }
            else if (settings.State == TenantState.Disabled)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating disabled shell context for tenant '{TenantName}'", settings.Name);
                }

                return Task.FromResult(new ShellContext { Settings = settings });
            }
            else if (settings.State == TenantState.Running || settings.State == TenantState.Initializing)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating shell context for tenant '{TenantName}'", settings.Name);
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
        private Task<ShellContext> CreateSetupContextAsync(ShellSettings defaultSettings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating shell context for root setup.");
            }

            if (defaultSettings == null)
            {
                // Creates a default shell settings based on the configuration.
                var shellSettings = _shellSettingsManager.CreateDefaultSettings();
                shellSettings.Name = ShellHelper.DefaultShellName;
                shellSettings.State = TenantState.Uninitialized;
                defaultSettings = shellSettings;
            }

            return _shellContextFactory.CreateSetupContextAsync(defaultSettings);
        }

        /// <summary>
        /// A feature is enabled / disabled, the tenant needs to be restarted
        /// </summary>
        public Task Changed(ShellDescriptor descriptor, string tenant)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("A tenant needs to be restarted '{TenantName}'", tenant);
            }

            if (_shellContexts.TryRemove(tenant, out var context))
            {
                context.Release();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Marks the specific tenant as released, such that a new shell is created for subsequent requests,
        /// while existing requests get flushed.
        /// </summary>
        /// <param name="settings"></param>
        public async Task ReloadShellContextAsync(ShellSettings settings)
        {
            if (settings.State == TenantState.Disabled)
            {
                // If a disabled shell is still in use it will be released and then disposed by its last scope.
                // So, we keep it in the list and don't create a new one that would have a null service provider.
                // Knowing that it is still removed from the running shell table, so that it is no more served.
                if (_shellContexts.TryGetValue(settings.Name, out var value) && value.ActiveScopes > 0)
                {
                    _runningShellTable.Remove(settings);
                    return;
                }
            }

            if (_shellContexts.TryRemove(settings.Name, out var context))
            {
                _runningShellTable.Remove(settings);
                context.Release();
            }

            if (settings.State != TenantState.Initializing)
            {
                settings = await _shellSettingsManager.LoadSettingsAsync(settings.Name);
            }

            await GetOrCreateShellContextAsync(settings);
        }

        public IEnumerable<ShellContext> ListShellContexts() => _shellContexts.Values.ToArray();

        /// <summary>
        /// Tries to retrieve the shell settings associated with the specified tenant.
        /// </summary>
        /// <returns><c>true</c> if the settings could be found, <c>false</c> otherwise.</returns>
        public bool TryGetSettings(string name, out ShellSettings settings)
        {
            if (_shellContexts.TryGetValue(name, out var shell))
            {
                settings = shell.Settings;
                return true;
            }

            settings = null;
            return false;
        }

        /// <summary>
        /// Retrieves all shell settings.
        /// </summary>
        /// <returns>All shell settings.</returns>
        public IEnumerable<ShellSettings> GetAllSettings() => ListShellContexts().Select(s => s.Settings);

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

        public void Dispose()
        {
            var shells = _shellContexts.Values.ToArray();

            foreach (var shell in shells)
            {
                shell.Dispose();
            }
        }
    }
}
