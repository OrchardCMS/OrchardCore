using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;
using OrchardCore.Extensions;
using OrchardCore.Tenant.Builders;
using OrchardCore.Tenant.Descriptor.Models;
using OrchardCore.Tenant.Models;
using Orchard.Hosting.TenantBuilders;

namespace OrchardCore.Tenant
{
    /// <summary>
    /// All <see cref="TenantContext"/> object are loaded when <see cref="Initialize"/> is called. They can be removed when the
    /// tenant is removed, but are necessary to match an incoming request, even if they are not initialized.
    /// Each <see cref="TenantContext"/> is activated (its service provider is built) on the first request.
    /// </summary>
    public class TenantHost : ITenantHost, ITenantDescriptorManagerEventHandler
    {
        private readonly ITenantSettingsManager _tenantSettingsManager;
        private readonly ITenantContextFactory _tenantContextFactory;
        private readonly IRunningTenantTable _runningTenantTable;
        private readonly ILogger _logger;

        private readonly static object _syncLock = new object();
        private ConcurrentDictionary<string, TenantContext> _tenantContexts;
        private readonly IExtensionManager _extensionManager;

        public TenantHost(
            ITenantSettingsManager tenantSettingsManager,
            ITenantContextFactory tenantContextFactory,
            IRunningTenantTable runningTenantTable,
            IExtensionManager extensionManager,
            ILogger<TenantHost> logger)
        {
            _extensionManager = extensionManager;
            _tenantSettingsManager = tenantSettingsManager;
            _tenantContextFactory = tenantContextFactory;
            _runningTenantTable = runningTenantTable;
            _logger = logger;
        }

        public void Initialize()
        {
            BuildCurrent();
        }

        /// <summary>
        /// Ensures tenants are activated, or re-activated if extensions have changed
        /// </summary>
        IDictionary<string, TenantContext> BuildCurrent()
        {
            if (_tenantContexts == null)
            {
                lock (this)
                {
                    if (_tenantContexts == null)
                    {
                        _tenantContexts = new ConcurrentDictionary<string, TenantContext>();
                        CreateAndRegisterTenantsAsync().Wait();
                    }
                }
            }

            return _tenantContexts;
        }

        public TenantContext GetOrCreateTenantContext(TenantSettings settings)
        {
            return _tenantContexts.GetOrAdd(settings.Name, tenant =>
            {
                var tenantContext = CreateTenantContextAsync(settings).Result;
                RegisterTenant(tenantContext);

                return tenantContext;
            });
        }

        public void UpdateTenantSettings(TenantSettings settings)
        {
            _tenantSettingsManager.SaveSettings(settings);
            ReloadTenantContext(settings);
        }

        async Task CreateAndRegisterTenantsAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Start creation of tenants");
            }

            // Load all extensions and features so that the controllers are
            // registered in ITypeFeatureProvider and their areas defined in the application
            // conventions.
            var features = _extensionManager.LoadFeaturesAsync();

            // Is there any tenant right now?
            var allSettings = _tenantSettingsManager.LoadSettings().Where(CanCreateTenant).ToArray();

            features.Wait();

            // No settings, run the Setup.
            if (allSettings.Length == 0)
            {
                var setupContext = await CreateSetupContextAsync();
                RegisterTenant(setupContext);
            }
            else
            {
                // Load all tenants, and activate their tenant.
                Parallel.ForEach(allSettings, settings =>
                {
                    try
                    {
                        GetOrCreateTenantContext(settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"A tenant could not be started: {settings.Name}", ex);

                        if (ex.IsFatal())
                        {
                            throw;
                        }
                    }
                });
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done creating tenants");
            }
        }

        /// <summary>
        /// Registers the tenant settings in RunningTenantTable
        /// </summary>
        private void RegisterTenant(TenantContext context)
        {
            if (!CanRegisterTenant(context.Settings))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Skipping tenant context registration for tenant {0}", context.Settings.Name);
                }

                return;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Registering tenant context for tenant {0}", context.Settings.Name);
            }

            if (_tenantContexts.TryAdd(context.Settings.Name, context))
            {
                _runningTenantTable.Add(context.Settings);
            }
        }

        /// <summary>
        /// Creates a tenant context based on tenant settings
        /// </summary>
        public async Task<TenantContext> CreateTenantContextAsync(TenantSettings settings)
        {
            if (settings.State == TenantState.Uninitialized)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating tenant context for tenant {0} setup", settings.Name);
                }

                return await _tenantContextFactory.CreateSetupContextAsync(settings);
            }
            else if (settings.State == TenantState.Disabled)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating disabled tenant context for tenant {0} setup", settings.Name);
                }

                return new TenantContext { Settings = settings };
            }
            else if(settings.State == TenantState.Running || settings.State == TenantState.Initializing)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Creating tenant context for tenant {0}", settings.Name);
                }

                return await _tenantContextFactory.CreateTenantContextAsync(settings);
            }
            else
            {
                throw new InvalidOperationException("Unexpected tenant state for " + settings.Name);
            }
        }

        /// <summary>
        /// Creates a transient tenant for the default tenant's setup.
        /// </summary>
        private Task<TenantContext> CreateSetupContextAsync()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Creating tenant context for root setup.");
            }

            return _tenantContextFactory.CreateSetupContextAsync(TenantHelper.BuildDefaultUninitializedTenant);
        }

        /// <summary>
        /// A feature is enabled/disabled, the tenant needs to be restarted
        /// </summary>
        Task ITenantDescriptorManagerEventHandler.Changed(TenantDescriptor descriptor, string tenant)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("A tenant needs to be restarted {0}", tenant);
            }

            if (_tenantContexts == null)
            {
                return Task.CompletedTask;
            }

            TenantContext context;
            if (!_tenantContexts.TryGetValue(tenant, out context))
            {
                return Task.CompletedTask;
            }

            if (_tenantContexts.TryRemove(tenant, out context))
            {
                context.Dispose();
            }

            return Task.CompletedTask;
        }

        public void ReloadTenantContext(TenantSettings settings)
        {
            TenantContext context;

            if (_tenantContexts.TryRemove(settings.Name, out context))
            {
                _runningTenantTable.Remove(settings);
                context.Dispose();
            }

            GetOrCreateTenantContext(settings);
        }

        public IEnumerable<TenantContext> ListTenantContexts()
        {
            return _tenantContexts.Values;
        }

        /// <summary>
        /// Whether or not a tenant can be added to the list of available tenants.
        /// </summary>
        private bool CanCreateTenant(TenantSettings tenantSettings)
        {
            return
                tenantSettings.State == TenantState.Running ||
                tenantSettings.State == TenantState.Uninitialized ||
                tenantSettings.State == TenantState.Initializing ||
                tenantSettings.State == TenantState.Disabled;
        }

        /// <summary>
        /// Whether or not a tenant can be activated and added to the running tenants.
        /// </summary>
        private bool CanRegisterTenant(TenantSettings tenantSettings)
        {
            return
                tenantSettings.State == TenantState.Running ||
                tenantSettings.State == TenantState.Uninitialized ||
                tenantSettings.State == TenantState.Initializing;
        }
}
}