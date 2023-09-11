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
using OrchardCore.Environment.Shell.Events;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// All <see cref="ShellContext"/> are pre-created when <see cref="InitializeAsync"/> is called on startup and where we first load
    /// all <see cref="ShellSettings"/> that we also need to register in the <see cref="IRunningShellTable"/> to serve incoming requests.
    /// For each <see cref="ShellContext"/> a service container and then a request pipeline are only built on the first matching request.
    /// </summary>
    public sealed class ShellHost : IShellHost, IDisposable, IAsyncDisposable
    {
        private const int ReloadShellMaxRetriesCount = 9;

        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;

        private bool _initialized;
        private readonly ConcurrentDictionary<string, ShellContext> _shellContexts = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ShellSettings> _shellSettings = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _shellSemaphores = new();
        private readonly SemaphoreSlim _initializingSemaphore = new(1);

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

        public ShellsEvent LoadingAsync { get; set; }
        public ShellEvent ReleasingAsync { get; set; }
        public ShellEvent ReloadingAsync { get; set; }
        public ShellEvent RemovingAsync { get; set; }

        /// <summary>
        /// Ensures that all the <see cref="ShellContext"/> are pre-created and available to process requests.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            // Prevent concurrent requests from creating all shells multiple times.
            await _initializingSemaphore.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    await PreCreateAndRegisterShellsAsync();
                    _initialized = true;
                }
            }
            finally
            {
                _initializingSemaphore.Release();
            }
        }

        /// <summary>
        /// Returns an existing <see cref="ShellContext"/> or creates a new one if necessary.
        /// </summary>
        public async Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings)
        {
            ShellContext shell = null;
            while (shell is null)
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
                    }
                }

                if (shell.Released)
                {
                    // If the context is released, it is removed from the dictionary so that the next iteration
                    // or a new call on 'GetOrCreateShellContextAsync()' will recreate a new shell context.
                    _shellContexts.TryRemove(settings.Name, out _);
                    shell = null;
                }
            }

            return shell;
        }

        /// <summary>
        /// Creates a standalone service scope that can be used to resolve local services.
        /// </summary>
        public async Task<ShellScope> GetScopeAsync(ShellSettings settings)
        {
            ShellScope scope = null;
            while (scope is null)
            {
                if (!_shellContexts.TryGetValue(settings.Name, out var shellContext))
                {
                    shellContext = await GetOrCreateShellContextAsync(settings);
                }

                // We create a scope before checking if the shell has been released.
                scope = await shellContext.CreateScopeAsync();

                // If 'CreateScope()' returned null, the shell is released. We then remove it and
                // retry with the hope to get one that won't be released before we create a scope.
                if (scope is null)
                {
                    // If the context is released, it is removed from the dictionary so that the next
                    // iteration or a new call on 'GetScopeAsync()' will recreate a new shell context.
                    _shellContexts.TryRemove(settings.Name, out _);
                }
            }

            return scope;
        }

        /// <summary>
        /// Updates an existing shell configuration and then reloads the shell.
        /// </summary>
        public async Task UpdateShellSettingsAsync(ShellSettings settings)
        {
            settings.VersionId = IdGenerator.GenerateId();
            await _shellSettingsManager.SaveSettingsAsync(settings);
            await ReloadShellContextAsync(settings);
        }

        /// <summary>
        /// Removes a shell context and its settings from memory and from the storage.
        /// </summary>
        public async Task RemoveShellSettingsAsync(ShellSettings settings)
        {
            CheckCanRemoveShell(settings);
            await _shellSettingsManager.RemoveSettingsAsync(settings);
            await RemoveShellContextAsync(settings);
        }

        /// <summary>
        /// A feature is enabled / disabled, the tenant needs to be released so that a new shell will be built.
        /// </summary>
        public Task ChangedAsync(ShellDescriptor descriptor, ShellSettings settings)
            => ReleaseShellContextAsync(settings);

        /// <summary>
        /// Reloads the settings and releases the shell so that a new one will be
        /// built for subsequent requests, while existing requests get flushed.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> to reload.</param>
        /// <param name="eventSource">Whether the related <see cref="ShellEvent"/> is invoked.</param>
        public async Task ReloadShellContextAsync(ShellSettings settings, bool eventSource = true)
        {
            if (ReloadingAsync is not null && eventSource && !settings.IsInitializing())
            {
                foreach (var d in ReloadingAsync.GetInvocationList())
                {
                    await ((ShellEvent)d)(settings.Name);
                }
            }

            // A disabled shell still in use will be released by its last scope.
            if (!CanReleaseShell(settings))
            {
                _runningShellTable.Remove(settings);
                return;
            }

            if (!settings.IsInitializing())
            {
                settings = await _shellSettingsManager.LoadSettingsAsync(settings.Name);
            }

            var count = 0;
            while (count++ < ReloadShellMaxRetriesCount)
            {
                if (_shellContexts.TryRemove(settings.Name, out var context))
                {
                    _runningShellTable.Remove(settings);
                    await context.ReleaseAsync();
                }

                // Add a 'PlaceHolder' allowing to retrieve the settings until the shell will be rebuilt.
                if (!_shellContexts.TryAdd(settings.Name, new ShellContext.PlaceHolder { Settings = settings }))
                {
                    // Atomicity: We may have been the last to load the settings but unable to add the shell.
                    continue;
                }

                _shellSettings[settings.Name] = settings;

                if (CanRegisterShell(settings))
                {
                    _runningShellTable.Add(settings);
                }

                if (settings.IsInitializing())
                {
                    return;
                }

                var currentVersionId = settings.VersionId;

                settings = await _shellSettingsManager.LoadSettingsAsync(settings.Name);

                // Consistency: We may have been the last to add the shell but not with the last settings.
                if (settings.VersionId == currentVersionId)
                {
                    return;
                }
            }

            throw new ShellHostReloadException(
                $"Unable to reload the tenant '{settings.Name}' as too many concurrent processes are trying to do so.");
        }

        /// <summary>
        /// Releases a shell so that a new one will be built for subsequent requests.
        /// Note: Can be used to free up resources after a given time of inactivity.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> to reload.</param>
        /// <param name="eventSource">Whether the related <see cref="ShellEvent"/> is invoked.</param>
        public async Task ReleaseShellContextAsync(ShellSettings settings, bool eventSource = true)
        {
            if (ReleasingAsync is not null && eventSource && !settings.IsInitializing())
            {
                foreach (var d in ReleasingAsync.GetInvocationList())
                {
                    await ((ShellEvent)d)(settings.Name);
                }
            }

            // A disabled shell still in use will be released by its last scope.
            if (!CanReleaseShell(settings))
            {
                return;
            }

            if (_shellContexts.TryRemove(settings.Name, out var context))
            {
                await context.ReleaseAsync();
            }

            // Add a 'PlaceHolder' allowing to retrieve the settings until the shell will be rebuilt.
            if (_shellContexts.TryAdd(settings.Name, new ShellContext.PlaceHolder { Settings = settings }))
            {
                _shellSettings[settings.Name] = settings;
            }
        }

        /// <summary>
        /// Removes a shell context and its settings but only from memory, used for syncing
        /// when the settings has been already removed from the storage by another instance.
        /// </summary>
        /// <param name="settings">The <see cref="ShellSettings"/> of the shell to remove.</param>
        /// <param name="eventSource">Whether the related <see cref="ShellEvent"/> is invoked.</param>
        public async Task RemoveShellContextAsync(ShellSettings settings, bool eventSource = true)
        {
            CheckCanRemoveShell(settings);

            if (RemovingAsync is not null && eventSource && !settings.IsInitializing())
            {
                foreach (var d in RemovingAsync.GetInvocationList())
                {
                    await ((ShellEvent)d)(settings.Name);
                }
            }

            if (_shellContexts.TryRemove(settings.Name, out var context))
            {
                await context.ReleaseAsync();
            }

            _shellSettings.TryRemove(settings.Name, out _);
        }

        /// <summary>
        /// Lists all available <see cref="ShellContext"/> instances.
        /// A shell might have been released or not yet built, if so 'shell.Released' is true and
        /// 'shell.CreateScopeAsync()' return null, but you can still use 'GetScopeAsync(shell.Settings)'.
        /// </summary>
        public IEnumerable<ShellContext> ListShellContexts() => _shellContexts.Values.ToArray();

        /// <summary>
        /// Tries to retrieve the shell context associated with the specified tenant.
        /// The shell may have been temporarily removed while releasing or reloading.
        /// </summary>
        public bool TryGetShellContext(string name, out ShellContext shellContext) => _shellContexts.TryGetValue(name, out shellContext);

        /// <summary>
        /// Tries to retrieve the shell settings associated with the specified tenant.
        /// </summary>
        public bool TryGetSettings(string name, out ShellSettings settings) => _shellSettings.TryGetValue(name, out settings);

        /// <summary>
        /// Retrieves all shell settings.
        /// </summary>
        public IEnumerable<ShellSettings> GetAllSettings() => _shellSettings.Values.ToArray();

        /// <summary>
        /// Pre-creates and registers all shells, if the 'Default' shell is not running, a setup context is built.
        /// On first loading only a placeholder is pre-created for each shell that is then fully built on request.
        /// </summary>
        private async Task PreCreateAndRegisterShellsAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start creation of shells");
            }

            // Load all extensions and features and register their components.
            await _extensionManager.LoadFeaturesAsync();

            if (LoadingAsync is not null)
            {
                foreach (var d in LoadingAsync.GetInvocationList())
                {
                    await ((ShellsEvent)d)();
                }
            }

            // Is there any tenant right now?
            var allSettings = (await _shellSettingsManager.LoadSettingsAsync()).Where(CanCreateShell).ToArray();
            var defaultSettings = allSettings.FirstOrDefault(s => s.IsDefaultShell());

            // The 'Default' tenant is not running, run the Setup.
            if (!defaultSettings.IsRunning())
            {
                var setupContext = await CreateSetupContextAsync(defaultSettings);
                AddAndRegisterShell(setupContext);
            }

            // Pre-create and register all tenant shells.
            foreach (var settings in allSettings)
            {
                AddAndRegisterShell(new ShellContext.PlaceHolder { Settings = settings, PreCreated = true });
            };

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done pre-creating and registering shells");
            }
        }

        /// <summary>
        /// Creates a shell context based on shell settings.
        /// </summary>
        private Task<ShellContext> CreateShellContextAsync(ShellSettings settings)
        {
            if (settings.IsUninitialized())
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating shell context for tenant '{TenantName}' setup", settings.Name);
                }

                return _shellContextFactory.CreateSetupContextAsync(settings);
            }
            else if (settings.IsDisabled())
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating disabled shell context for tenant '{TenantName}'", settings.Name);
                }

                return Task.FromResult(new ShellContext { Settings = settings });
            }
            else if (settings.IsRunning() || settings.IsInitializing())
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
        private async Task<ShellContext> CreateSetupContextAsync(ShellSettings defaultSettings)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating shell context for root setup.");
            }

            if (defaultSettings is null)
            {
                // Creates a default shell settings based on the configuration.
                defaultSettings = _shellSettingsManager
                    .CreateDefaultSettings()
                    .AsDefaultShell()
                    .AsUninitialized();

                await UpdateShellSettingsAsync(defaultSettings);
            }

            return await _shellContextFactory.CreateSetupContextAsync(defaultSettings);
        }

        /// <summary>
        /// Adds the shell and registers its settings in 'RunningShellTable'.
        /// </summary>
        private void AddAndRegisterShell(ShellContext context)
        {
            if (_shellContexts.TryAdd(context.Settings.Name, context))
            {
                _shellSettings[context.Settings.Name] = context.Settings;

                if (CanRegisterShell(context))
                {
                    RegisterShellSettings(context.Settings);
                }
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
        /// Registers the shell settings in 'RunningShellTable'.
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
        /// Whether or not a shell can be added to the list of available shells.
        /// </summary>
        private static bool CanCreateShell(ShellSettings shellSettings) =>
            shellSettings.IsRunning() ||
            shellSettings.IsUninitialized() ||
            shellSettings.IsInitializing() ||
            shellSettings.IsDisabled();

        /// <summary>
        /// Whether or not a shell can be activated and added to the running shells.
        /// </summary>
        private static bool CanRegisterShell(ShellSettings shellSettings) =>
            shellSettings.IsRunning() ||
            shellSettings.IsUninitialized() ||
            shellSettings.IsInitializing();

        /// <summary>
        /// Whether or not a shell can be released and removed from the list, false if disabled and still in use.
        /// Note: A disabled shell still in use will be released by its last scope, and keeping it in the list
        /// prevents a consumer from creating a new one that would have a null service provider.
        /// </summary>
        private bool CanReleaseShell(ShellSettings settings) => !settings.IsDisabled() || !IsShellActive(settings);

        /// <summary>
        /// Checks if a shell can be removed, throws an exception if the shell is neither uninitialized nor disabled.
        /// </summary>
        private void CheckCanRemoveShell(ShellSettings settings)
        {
            if (settings.IsDefaultShell())
            {
                throw new InvalidOperationException($"The '{ShellSettings.DefaultShellName}' tenant can't be removed.");
            }

            // A disabled shell may be still in use in at least one active scope.
            if (!settings.IsRemovable() || IsShellActive(settings))
            {
                throw new InvalidOperationException(
                    $"The tenant '{settings.Name}' can't be removed as it is neither uninitialized nor disabled.");
            }
        }

        /// <summary>
        /// Whether or not a shell is in use in at least one active scope.
        /// </summary>
        private bool IsShellActive(ShellSettings settings) =>
            settings is { Name: not null } &&
            _shellContexts.TryGetValue(settings.Name, out var context) &&
            context.IsActive();

        public void Dispose()
        {
            foreach (var shell in ListShellContexts())
            {
                shell.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var shell in ListShellContexts())
            {
                await shell.DisposeAsync();
            }
        }
    }
}
