using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Data.YesSql;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Setup.Events;
using YesSql;

namespace OrchardCore.Setup.Services
{
    /// <summary>
    /// Represents a setup service.
    /// </summary>
    public class SetupService : ISetupService
    {
        private readonly IShellHost _shellHost;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly ISetupUserIdGenerator _setupUserIdGenerator;
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;
        private readonly ILogger _logger;
        private readonly IStringLocalizer S;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbConnectionValidator _dbConnectionValidator;
        private readonly string _applicationName;
        private IEnumerable<RecipeDescriptor> _recipes;

        /// <summary>
        /// Creates a new instance of <see cref="SetupService"/>.
        /// </summary>
        /// <param name="shellHost">The <see cref="IShellHost"/>.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostEnvironment"/>.</param>
        /// <param name="shellContextFactory">The <see cref="IShellContextFactory"/>.</param>
        /// <param name="setupUserIdGenerator">The <see cref="ISetupUserIdGenerator"/>.</param>
        /// <param name="recipeHarvesters">A list of <see cref="IRecipeHarvester"/>s.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
        /// <param name="applicationLifetime">The <see cref="IHostApplicationLifetime"/>.</param>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/>.</param>
        /// <param name="dbConnectionValidator">The <see cref="IDbConnectionValidator"/>.</param>
        public SetupService(
            IShellHost shellHost,
            IHostEnvironment hostingEnvironment,
            IShellContextFactory shellContextFactory,
            ISetupUserIdGenerator setupUserIdGenerator,
            IEnumerable<IRecipeHarvester> recipeHarvesters,
            ILogger<SetupService> logger,
            IStringLocalizer<SetupService> stringLocalizer,
            IHostApplicationLifetime applicationLifetime,
            IHttpContextAccessor httpContextAccessor,
            IDbConnectionValidator dbConnectionValidator)
        {
            _shellHost = shellHost;
            _applicationName = hostingEnvironment.ApplicationName;
            _shellContextFactory = shellContextFactory;
            _setupUserIdGenerator = setupUserIdGenerator;
            _recipeHarvesters = recipeHarvesters;
            _logger = logger;
            S = stringLocalizer;
            _applicationLifetime = applicationLifetime;
            _httpContextAccessor = httpContextAccessor;
            _dbConnectionValidator = dbConnectionValidator;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync()
        {
            if (_recipes == null)
            {
                var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
                _recipes = recipeCollections.SelectMany(x => x).Where(x => x.IsSetupRecipe).ToArray();
            }

            return _recipes;
        }

        /// <inheritdoc />
        public async Task<string> SetupAsync(SetupContext context)
        {
            var initialState = context.ShellSettings.State;
            try
            {
                var executionId = await SetupInternalAsync(context);

                if (context.Errors.Any())
                {
                    context.ShellSettings.State = initialState;
                    await _shellHost.ReloadShellContextAsync(context.ShellSettings, eventSource: false);
                }

                return executionId;
            }
            catch
            {
                context.ShellSettings.State = initialState;
                await _shellHost.ReloadShellContextAsync(context.ShellSettings, eventSource: false);

                throw;
            }
        }

        private async Task<string> SetupInternalAsync(SetupContext context)
        {
            string executionId;

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Running setup for tenant '{TenantName}'.", context.ShellSettings.Name);
            }

            // Features to enable for Setup
            string[] hardcoded =
            {
                _applicationName,
                "OrchardCore.Features",
                "OrchardCore.Scripting",
                "OrchardCore.Recipes"
            };

            context.EnabledFeatures = hardcoded.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            // Set shell state to "Initializing" so that subsequent HTTP requests are responded to with "Service Unavailable" while Orchard is setting up.
            context.ShellSettings.State = TenantState.Initializing;

            // Due to database collation we normalize the userId to lower invariant.
            // During setup there are no users so we do not need to check unicity.
            context.Properties[SetupConstants.AdminUserId] = _setupUserIdGenerator.GenerateUniqueId().ToLowerInvariant();

            var recipeEnvironmentFeature = new RecipeEnvironmentFeature();
            if (context.Properties.TryGetValue(SetupConstants.AdminUserId, out var adminUserId))
            {
                recipeEnvironmentFeature.Properties[SetupConstants.AdminUserId] = adminUserId;
            }
            if (context.Properties.TryGetValue(SetupConstants.AdminUsername, out var adminUsername))
            {
                recipeEnvironmentFeature.Properties[SetupConstants.AdminUsername] = adminUsername;
            }
            if (context.Properties.TryGetValue(SetupConstants.SiteName, out var siteName))
            {
                recipeEnvironmentFeature.Properties[SetupConstants.SiteName] = siteName;
            }

            _httpContextAccessor.HttpContext.Features.Set(recipeEnvironmentFeature);

            var shellSettings = new ShellSettings(context.ShellSettings);

            if (String.IsNullOrWhiteSpace(shellSettings["DatabaseProvider"]))
            {
                shellSettings["DatabaseProvider"] = context.Properties.TryGetValue(SetupConstants.DatabaseProvider, out var databaseProvider) ? databaseProvider?.ToString() : String.Empty;
                shellSettings["ConnectionString"] = context.Properties.TryGetValue(SetupConstants.DatabaseConnectionString, out var databaseConnectionString) ? databaseConnectionString?.ToString() : String.Empty;
                shellSettings["TablePrefix"] = context.Properties.TryGetValue(SetupConstants.DatabaseTablePrefix, out var databaseTablePrefix) ? databaseTablePrefix?.ToString() : String.Empty;
            }

            switch (await _dbConnectionValidator.ValidateAsync(shellSettings["DatabaseProvider"], shellSettings["ConnectionString"], shellSettings["TablePrefix"], shellSettings.Name))
            {
                case DbConnectionValidatorResult.NoProvider:
                    context.Errors.Add(String.Empty, S["DatabaseProvider setting is required."]);
                    break;
                case DbConnectionValidatorResult.UnsupportedProvider:
                    context.Errors.Add(String.Empty, S["The provided database provider is not supported."]);
                    break;
                case DbConnectionValidatorResult.InvalidConnection:
                    context.Errors.Add(String.Empty, S["The provided connection string is invalid or server is unreachable."]);
                    break;
                case DbConnectionValidatorResult.DocumentTableFound:
                    context.Errors.Add(String.Empty, S["The provided database and table prefix are already in use."]);
                    break;
            }

            if (context.Errors.Any())
            {
                return null;
            }

            // Creating a standalone environment based on a "minimum shell descriptor".
            // In theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect
            // It is used to initialize the database before the recipe is run.

            var shellDescriptor = new ShellDescriptor
            {
                Features = context.EnabledFeatures.Select(id => new ShellFeature { Id = id }).ToList()
            };

            using (var shellContext = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, shellDescriptor))
            {
                await shellContext.CreateScope().UsingServiceScopeAsync(async scope =>
                {
                    IStore store;

                    try
                    {
                        store = scope.ServiceProvider.GetRequiredService<IStore>();
                    }
                    catch (Exception e)
                    {
                        // Tables already exist or database was not found

                        // The issue is that the user creation needs the tables to be present,
                        // if the user information is not valid, the next POST will try to recreate the
                        // tables. The tables should be rolled back if one of the steps is invalid,
                        // unless the recipe is executing?

                        _logger.LogError(e, "An error occurred while initializing the datastore.");
                        context.Errors.Add(String.Empty, S["An error occurred while initializing the datastore: {0}", e.Message]);
                        return;
                    }

                    // Create the "minimum shell descriptor"
                    await scope
                        .ServiceProvider
                        .GetService<IShellDescriptorManager>()
                        .UpdateShellDescriptorAsync(0,
                            shellContext.Blueprint.Descriptor.Features);
                });

                if (context.Errors.Any())
                {
                    return null;
                }

                executionId = Guid.NewGuid().ToString("n");

                var recipeExecutor = shellContext.ServiceProvider.GetRequiredService<IRecipeExecutor>();

                await recipeExecutor.ExecuteAsync(executionId, context.Recipe, context.Properties, _applicationLifetime.ApplicationStopping);
            }

            // Reloading the shell context as the recipe has probably updated its features
            await (await _shellHost.GetScopeAsync(shellSettings)).UsingAsync(async scope =>
            {
                void reportError(string key, string message)
                {
                    context.Errors[key] = message;
                }

                // Invoke modules to react to the setup event
                var setupEventHandlers = scope.ServiceProvider.GetServices<ISetupEventHandler>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SetupService>>();

                await setupEventHandlers.InvokeAsync((handler, context) => handler.Setup(
                    context.Properties,
                    reportError
                ), context, logger);
            });

            if (context.Errors.Any())
            {
                return executionId;
            }

            // Update the shell state
            shellSettings.State = TenantState.Running;
            await _shellHost.UpdateShellSettingsAsync(shellSettings);

            return executionId;
        }
    }
}
