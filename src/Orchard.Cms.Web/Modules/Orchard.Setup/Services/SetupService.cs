using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using OrchardCore.Extensions;
using OrchardCore.Tenant;
using OrchardCore.Tenant.Builders;
using OrchardCore.Tenant.Descriptor;
using OrchardCore.Tenant.Descriptor.Models;
using OrchardCore.Tenant.Models;
using Orchard.Events;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;
using Microsoft.Extensions.Localization;

namespace Orchard.Setup.Services
{
    public class SetupService : ISetupService
    {
        private readonly TenantSettings _tenantSettings;
        private readonly ITenantHost _orchardHost;
        private readonly ITenantContextFactory _tenantContextFactory;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IExtensionManager _extensionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRunningTenantTable _runningTenantTable;
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly ILogger _logger;
        private readonly IStringLocalizer T;

        private IReadOnlyList<RecipeDescriptor> _recipes;

        public SetupService(
            TenantSettings tenantSettings,
            ITenantHost orchardHost,
            ITenantContextFactory tenantContextFactory,
            ICompositionStrategy compositionStrategy,
            IExtensionManager extensionManager,
            IHttpContextAccessor httpContextAccessor,
            IRunningTenantTable runningTenantTable,
            IRecipeHarvester recipeHarvester,
            ILogger<SetupService> logger,
            IStringLocalizer<SetupService> stringLocalizer
            )
        {
            _tenantSettings = tenantSettings;
            _orchardHost = orchardHost;
            _tenantContextFactory = tenantContextFactory;
            _compositionStrategy = compositionStrategy;
            _extensionManager = extensionManager;
            _httpContextAccessor = httpContextAccessor;
            _runningTenantTable = runningTenantTable;
            _recipeHarvester = recipeHarvester;
            _logger = logger;
            T = stringLocalizer;
        }

        public async Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
        {
            if (_recipes == null)
            {
                _recipes = (await _recipeHarvester.HarvestRecipesAsync())
                    .Where(recipe => recipe.IsSetupRecipe)
                    .ToList();
            }

            return _recipes;
        }

        public async Task<string> SetupAsync(SetupContext context)
        {
            var initialState = _tenantSettings.State;
            try
            {
                return await SetupInternalAsync(context);
            }
            catch
            {
                _tenantSettings.State = initialState;
                throw;
            }
        }

        public async Task<string> SetupInternalAsync(SetupContext context)
        {
            string executionId;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Running setup for tenant '{0}'.", _tenantSettings.Name);
            }

            // Features to enable for Setup
            string[] hardcoded =
            {
                "Orchard.Commons",
                "Orchard.Modules",
                "Orchard.Recipes",
                "Orchard.Scripting"
            };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            // Set tenant state to "Initializing" so that subsequent HTTP requests are responded to with "Service Unavailable" while Orchard is setting up.
            _tenantSettings.State = TenantState.Initializing;

            var tenantSettings = new TenantSettings(_tenantSettings);

            if (string.IsNullOrEmpty(tenantSettings.DatabaseProvider))
            {
                tenantSettings.DatabaseProvider = context.DatabaseProvider;
                tenantSettings.ConnectionString = context.DatabaseConnectionString;
                tenantSettings.TablePrefix = context.DatabaseTablePrefix;
            }

            // Creating a standalone environment based on a "minimum tenant descriptor".
            // In theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect
            // It is used to initialize the database before the recipe is run.

            var tenantDescriptor = new TenantDescriptor
            {
                Features = context.EnabledFeatures.Select(id => new TenantFeature { Id = id }).ToList()
            };

            using (var tenantContext = await _tenantContextFactory.CreateDescribedContextAsync(tenantSettings, tenantDescriptor))
            {
                using (var scope = tenantContext.CreateServiceScope())
                {
                    var store = scope.ServiceProvider.GetRequiredService<IStore>();

                    try
                    {
                        await store.InitializeAsync();
                    }
                    catch(Exception e)
                    {
                        // Tables already exist or database was not found

                        // The issue is that the user creation needs the tables to be present,
                        // if the user information is not valid, the next POST will try to recreate the
                        // tables. The tables should be rolled back if one of the steps is invalid,
                        // unless the recipe is executing?

                        _logger.LogError("An error occurred while initializing the datastore.", e);
                        context.Errors.Add("DatabaseProvider", T["An error occurred while initializing the datastore: {0}", e.Message]);
                        return null;
                    }

                    // Create the "minimum tenant descriptor"
                    await scope
                        .ServiceProvider
                        .GetService<ITenantDescriptorManager>()
                        .UpdateTenantDescriptorAsync(0,
                            tenantContext.Blueprint.Descriptor.Features,
                            tenantContext.Blueprint.Descriptor.Parameters);

                    var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                    if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                    {
                        var taskContext = new DeferredTaskContext(scope.ServiceProvider);
                        await deferredTaskEngine.ExecuteTasksAsync(taskContext);
                    }
                }

                executionId = Guid.NewGuid().ToString("n");

                // Create a new scope for the recipe thread to prevent race issues with other scoped
                // services from the request.
                using (var scope = tenantContext.CreateServiceScope())
                {
                    var recipeExecutor = scope.ServiceProvider.GetService<IRecipeExecutor>();

                    // Right now we run the recipe in the same thread, later use polling from the setup screen
                    // to query the current execution.
                    //await Task.Run(async () =>
                    //{
                    await recipeExecutor.ExecuteAsync(executionId, context.Recipe, new
                    {
                        SiteName  = context.SiteName,
                        AdminUsername = context.AdminUsername,
                        AdminEmail = context.AdminEmail,
                        AdminPassword = context.AdminPassword,
                        DatabaseProvider = context.DatabaseProvider,
                        DatabaseConnectionString = context.DatabaseConnectionString,
                        DatabaseTablePrefix = context.DatabaseTablePrefix
                    });
                    //});

                }
            }

            // Reloading the tenant context as the recipe  has probably updated its features
            using (var tenantContext = await _orchardHost.CreateTenantContextAsync(tenantSettings))
            {
                using (var scope = tenantContext.CreateServiceScope())
                {
                    bool hasErrors = false;

                    Action<string, string> reportError = (key, message) => {
                        hasErrors = true;
                        context.Errors[key] = message;
                    };

                    // Invoke modules to react to the setup event
                    var eventBus = scope.ServiceProvider.GetService<IEventBus>();
                    await eventBus.NotifyAsync<ISetupEventHandler>(x => x.Setup(
                        context.SiteName,
                        context.AdminUsername,
                        context.AdminEmail,
                        context.AdminPassword,
                        context.DatabaseProvider,
                        context.DatabaseConnectionString,
                        context.DatabaseTablePrefix,
                        reportError
                    ));

                    if (hasErrors)
                    {
                        return executionId;
                    }

                    var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                    if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                    {
                        var taskContext = new DeferredTaskContext(scope.ServiceProvider);
                        await deferredTaskEngine.ExecuteTasksAsync(taskContext);
                    }
                }
            }

            // Update the tenant state
            tenantSettings.State = TenantState.Running;
            _orchardHost.UpdateTenantSettings(tenantSettings);

            return executionId;
        }
    }
}