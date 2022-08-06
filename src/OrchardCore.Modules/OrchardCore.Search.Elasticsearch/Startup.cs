using System;
using System.Linq;
using Elasticsearch.Net;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Search.Abstractions.ViewModels;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Drivers;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        private const string ConfigSectionName = "OrchardCore_Elasticsearch";
        private readonly AdminOptions _adminOptions;
        private readonly IShellConfiguration _shellConfiguration;
        private readonly ILogger<Startup> _logger;

        public Startup(IOptions<AdminOptions> adminOptions,
            IShellConfiguration shellConfiguration,
            ILogger<Startup> logger)
        {
            _adminOptions = adminOptions.Value;
            _shellConfiguration = shellConfiguration;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var elasticConfiguration = _shellConfiguration.GetSection(ConfigSectionName).Get<ElasticConnectionOptions>();

            if (elasticConfiguration != null && CheckOptions(elasticConfiguration.Url, elasticConfiguration.Ports, _logger))
            {
                services.Configure<TemplateOptions>(o =>
                {
                    o.MemberAccessStrategy.Register<SearchIndexViewModel>();
                    o.MemberAccessStrategy.Register<SearchFormViewModel>();
                    o.MemberAccessStrategy.Register<SearchResultsViewModel>();
                });

                services.Configure<ElasticConnectionOptions>(o => o.ConfigurationExists = true);
                services.AddScoped<INavigationProvider, AdminMenu>();
                services.AddScoped<IPermissionProvider, Permissions>();

                IConnectionPool pool = null;
                var uris = elasticConfiguration.Ports.Select(port => new Uri($"{elasticConfiguration.Url}:{port}")).Distinct();

                switch (elasticConfiguration.ConnectionType)
                {
                    case "SingleNodeConnectionPool":
                        pool = new SingleNodeConnectionPool(uris.First());
                        break;

                    case "CloudConnectionPool":
                        BasicAuthenticationCredentials credentials = null;

                        if (!String.IsNullOrWhiteSpace(elasticConfiguration.Username) && !String.IsNullOrWhiteSpace(elasticConfiguration.Password) && !String.IsNullOrWhiteSpace(elasticConfiguration.CloudId))
                        {
                            credentials = new BasicAuthenticationCredentials(elasticConfiguration.Username, elasticConfiguration.Password);
                            pool = new CloudConnectionPool(elasticConfiguration.CloudId, credentials);
                        }
                        break;

                    case "StaticConnectionPool":
                        pool = new StaticConnectionPool(uris);
                        break;

                    case "SniffingConnectionPool":
                        pool = new SniffingConnectionPool(uris);
                        break;

                    case "StickyConnectionPool":
                        pool = new StickyConnectionPool(uris);
                        break;

                    default:
                        pool = new SingleNodeConnectionPool(uris.First());
                        break;
                }

                var settings = new ConnectionSettings(pool).ThrowExceptions();

                if (elasticConfiguration.ConnectionType != "CloudConnectionPool" && !String.IsNullOrWhiteSpace(elasticConfiguration.Username) && !String.IsNullOrWhiteSpace(elasticConfiguration.Password))
                {
                    settings.BasicAuthentication(elasticConfiguration.Username, elasticConfiguration.Password);
                }

                if (!String.IsNullOrWhiteSpace(elasticConfiguration.CertificateFingerprint))
                {
                    settings.CertificateFingerprint(elasticConfiguration.CertificateFingerprint);
                }

                if (elasticConfiguration.EnableApiVersioningHeader)
                {
                    settings.EnableApiVersioningHeader();
                }

                var client = new ElasticClient(settings);
                services.AddSingleton<IElasticClient>(client);
                services.Configure<ElasticOptions>(o =>
                    o.Analyzers.Add(new ElasticAnalyzer(ElasticSettings.StandardAnalyzer, new StandardAnalyzer())));

                services.AddScoped<IDisplayDriver<ISite>, ElasticSettingsDisplayDriver>();
                services.AddScoped<IDisplayDriver<Query>, ElasticQueryDisplayDriver>();
                services.AddElasticServices();
            }
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<ElasticConnectionOptions>>().Value;

            if (!options.ConfigurationExists)
            {
                return;
            }

            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Elasticsearch.Index",
                areaName: "OrchardCore.Search.Elasticsearch",
                pattern: _adminOptions.AdminUrlPrefix + "/elasticsearch/Index",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Elasticsearch.Delete",
                areaName: "OrchardCore.Search.Elasticsearch",
                pattern: _adminOptions.AdminUrlPrefix + "/elasticsearch/Delete/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "Elasticsearch.Query",
                areaName: "OrchardCore.Search.Elasticsearch",
                pattern: _adminOptions.AdminUrlPrefix + "/elasticsearch/Query",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Query) }
            );

            routes.MapAreaControllerRoute(
                name: "Elasticsearch.Rebuild",
                areaName: "OrchardCore.Search.Elasticsearch",
                pattern: _adminOptions.AdminUrlPrefix + "/elasticsearch/Rebuild/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Rebuild) }
            );

            routes.MapAreaControllerRoute(
                name: "Elasticsearch.Reset",
                areaName: "OrchardCore.Search.Elasticsearch",
                pattern: _adminOptions.AdminUrlPrefix + "/elasticsearch/Reset/{id}",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Reset) }
            );
        }

        private static bool CheckOptions(string url, int[] ports, ILogger logger)
        {
            var optionsAreValid = true;

            if (String.IsNullOrWhiteSpace(url))
            {
                logger.LogError("Elasticsearch is enabled but not active because the 'Url' is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            if (ports.Length == 0)
            {
                logger.LogError("Elasticsearch is enabled but not active because a port is missing in application configuration.");
                optionsAreValid = false;
            }

            return optionsAreValid;
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, ElasticIndexDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ElasticIndexDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ElasticIndexDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, ElasticSettingsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ElasticSettingsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ElasticSettingsDeploymentStepDriver>();
        }
    }

    [Feature("OrchardCore.Search.Elasticsearch.ContentPicker")]
    public class ElasticContentPickerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPickerResultProvider, ElasticContentPickerResultProvider>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPickerFieldElasticEditorSettingsDriver>();
            services.AddShapeAttributes<ElasticContentPickerShapeProvider>();
        }
    }
}
