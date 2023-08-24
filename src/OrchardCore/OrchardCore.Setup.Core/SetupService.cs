using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Setup.Events;

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
        protected readonly IStringLocalizer S;
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
            if (_recipes is null)
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

                if (context.Errors.Count > 0)
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
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Running setup for tenant '{TenantName}'.", context.ShellSettings?.Name);
            }

            // Features to enable for Setup.
            string[] coreFeatures =
            {
                _applicationName,
                "OrchardCore.Features",
                "OrchardCore.Scripting",
                "OrchardCore.Recipes"
            };

            context.EnabledFeatures = coreFeatures.Union(context.EnabledFeatures ?? Enumerable.Empty<string>()).Distinct().ToList();

            // Set shell state to "Initializing" so that subsequent HTTP requests are responded to with "Service Unavailable" while Orchard is setting up.
            context.ShellSettings.AsInitializing();

            // Due to database collation we normalize the userId to lower invariant.
            // During setup there are no users so we do not need to check unicity.
            var adminUserId = _setupUserIdGenerator.GenerateUniqueId().ToLowerInvariant();
            context.Properties[SetupConstants.AdminUserId] = adminUserId;

            var recipeEnvironmentFeature = new RecipeEnvironmentFeature();
            recipeEnvironmentFeature.Properties[SetupConstants.AdminUserId] = adminUserId;

            if (context.Properties.TryGetValue(SetupConstants.AdminUsername, out var adminUsername))
            {
                recipeEnvironmentFeature.Properties[SetupConstants.AdminUsername] = adminUsername;
            }

            if (context.Properties.TryGetValue(SetupConstants.SiteName, out var siteName))
            {
                recipeEnvironmentFeature.Properties[SetupConstants.SiteName] = siteName;
            }

            _httpContextAccessor.HttpContext.Features.Set(recipeEnvironmentFeature);

            var shellSettings = new ShellSettings(context.ShellSettings).ConfigureDatabaseTableOptions();
            if (String.IsNullOrWhiteSpace(shellSettings["DatabaseProvider"]))
            {
                shellSettings["DatabaseProvider"] = context.Properties.TryGetValue(SetupConstants.DatabaseProvider, out var databaseProvider) ? databaseProvider?.ToString() : String.Empty;
                shellSettings["ConnectionString"] = context.Properties.TryGetValue(SetupConstants.DatabaseConnectionString, out var databaseConnectionString) ? databaseConnectionString?.ToString() : String.Empty;
                shellSettings["TablePrefix"] = context.Properties.TryGetValue(SetupConstants.DatabaseTablePrefix, out var databaseTablePrefix) ? databaseTablePrefix?.ToString() : String.Empty;
                shellSettings["Schema"] = context.Properties.TryGetValue(SetupConstants.DatabaseSchema, out var schema) ? schema?.ToString() : null;
            }

            var validationContext = new DbConnectionValidatorContext(shellSettings);
            switch (await _dbConnectionValidator.ValidateAsync(validationContext))
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
                    context.Errors.Add(String.Empty, S["The provided database, table prefix and schema are already in use."]);
                    break;
            }

            if (context.Errors.Count > 0)
            {
                return null;
            }

            // Creating a standalone environment based on a "minimum shell descriptor".
            // In theory this environment can be used to resolve any normal components by interface, and those
            // components will exist entirely in isolation - no crossover between the safemode container currently in effect
            // It is used to initialize the database before the recipe is run.
            var shellDescriptor = new ShellDescriptor
            {
                Features = context.EnabledFeatures.Select(id => new ShellFeature(id)).ToList()
            };

            string executionId;

            await using (var shellContext = await _shellContextFactory.CreateDescribedContextAsync(shellSettings, shellDescriptor))
            {
                await (await shellContext.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
                {
                    try
                    {
                        // Create the "minimum" shell descriptor.
                        await scope.ServiceProvider.GetService<IShellDescriptorManager>()
                            .UpdateShellDescriptorAsync(0, shellContext.Blueprint.Descriptor.Features);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An error occurred while initializing the datastore.");
                        context.Errors.Add(String.Empty, S["An error occurred while initializing the datastore: {0}", e.Message]);
                    }
                });

                if (context.Errors.Count > 0)
                {
                    return null;
                }

                executionId = Guid.NewGuid().ToString("n");

                var recipeExecutor = shellContext.ServiceProvider.GetRequiredService<IRecipeExecutor>();

                await recipeExecutor.ExecuteAsync(executionId, context.Recipe, context.Properties, _applicationLifetime.ApplicationStopping);
            }

            // Reloading the shell context as the recipe has probably updated its features.
            await (await _shellHost.GetScopeAsync(shellSettings)).UsingAsync(async scope =>
            {
                var handlers = scope.ServiceProvider.GetServices<ISetupEventHandler>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<SetupService>>();

                await handlers.InvokeAsync((handler, ctx) => handler.SetupAsync(ctx), context, _logger);

                if (context.Errors.Count > 0)
                {
                    await handlers.InvokeAsync((handler) => handler.FailedAsync(context), _logger);
                }
            });

            if (context.Errors.Count > 0)
            {
                return executionId;
            }

            // Update the shell state.
            await _shellHost.UpdateShellSettingsAsync(shellSettings.AsRunning());

            await (await _shellHost.GetScopeAsync(shellSettings)).UsingAsync(async scope =>
            {
                var handlers = scope.ServiceProvider.GetServices<ISetupEventHandler>();

                await handlers.InvokeAsync((handler) => handler.SucceededAsync(), _logger);
            });

            return executionId;
        }
    }
}
