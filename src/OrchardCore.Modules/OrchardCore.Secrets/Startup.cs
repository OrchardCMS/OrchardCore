using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Secrets.Controllers;
using OrchardCore.Secrets.Deployment;
using OrchardCore.Secrets.Drivers;
using OrchardCore.Secrets.Filters;
using OrchardCore.Secrets.Recipes;
using OrchardCore.Secrets.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Secrets
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<SecretBindingsManager>();

            services.AddScoped<ISecretCoordinator, DefaultSecretCoordinator>();
            services.AddScoped<ISecretExpressionEvaluator, SecretExpressionEvaluator>();

            services.AddScoped<IDisplayManager<Secret>, DisplayManager<Secret>>();

            services.AddScoped<IDisplayDriver<Secret>, AuthorizationSecretDisplayDriver>();
            services.AddSingleton<ISecretFactory>(new SecretFactory<AuthorizationSecret>());
            services.AddScoped<ISecretService<AuthorizationSecret>, SecretService<AuthorizationSecret>>();
            services.AddSecretLiquidFilter<AuthorizationSecretFilter>("auth_secret");


            services.AddScoped<IDisplayDriver<Secret>, RsaSecretDisplayDriver>();
            services.AddSingleton<ISecretFactory>(new SecretFactory<RsaSecret>());
            services.AddScoped<ISecretService<RsaSecret>, SecretService<RsaSecret>>();

            services.AddScoped<IEncryptionService, DefaultEncryptionService>();
            services.AddScoped<IDecryptionService, DefaultDecryptionService>();

            services.AddRecipeExecutionStep<SecretsRecipeStep>();

            services.AddTransient<IDeploymentSource, AllSecretsRsaDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllSecretsRsaDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllSecretsRsaDeploymentStepDriver>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var secretControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Secrets.Index",
                areaName: "OrchardCore.Secrets",
                pattern: _adminOptions.AdminUrlPrefix + "/Secrets",
                defaults: new { controller = secretControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Secrets.Create",
                areaName: "OrchardCore.Secrets",
                pattern: _adminOptions.AdminUrlPrefix + "/Secrets/Create",
                defaults: new { controller = secretControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Secrets.Edit",
                areaName: "OrchardCore.Secrets",
                pattern: _adminOptions.AdminUrlPrefix + "/Secrets/Edit/{name}",
                defaults: new { controller = secretControllerName, action = nameof(AdminController.Edit) }
            );
        }
    }

    [Feature("OrchardCore.Secrets.DatabaseSecretStore")]
    public class DatabaseSecretStoreStartup : StartupBase
    {
        // Registered first so it is the first store.
        public override int Order => -10;
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DatabaseSecretDataProtector>();

            services.AddScoped<ISecretStore, DatabaseSecretStore>();
            services.AddScoped<SecretsDocumentManager>();
        }
    }

    [Feature("OrchardCore.Secrets.ConfigurationSecretStore")]
    public class ConfigurationSecretStoreStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISecretStore, ConfigurationSecretStore>();
        }
    }
}
