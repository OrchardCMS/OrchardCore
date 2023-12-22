using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elasticsearch.Net;
using Fluid;
using GraphQL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks;
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
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Providers;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Drivers;
using OrchardCore.Search.Elasticsearch.Services;
using OrchardCore.Search.Lucene.Handler;
using OrchardCore.Search.ViewModels;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch
{
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
            var configuration = _shellConfiguration.GetSection(ConfigSectionName);
            var elasticConfiguration = configuration.Get<ElasticConnectionOptions>();

            if (!CheckOptions(elasticConfiguration, _logger))
            {
                return;
            }

            services.Configure<ElasticConnectionOptions>(o => o.ConfigurationExists = true);
            var settings = GetConnectionSettings(elasticConfiguration);

            services.AddSingleton<IElasticClient>(new ElasticClient(settings));

            services.Configure<ElasticsearchOptions>(o =>
            {
                o.IndexPrefix = configuration.GetValue<string>(nameof(o.IndexPrefix));

                var jsonNode = configuration.GetSection(nameof(o.Analyzers)).AsJsonNode();
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonNode);

                var analyzersObject = JsonObject.Create(jsonElement, new JsonNodeOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (analyzersObject != null)
                {
                    foreach (var analyzer in analyzersObject)
                    {
                        if (analyzer.Value == null)
                        {
                            continue;
                        }

                        o.Analyzers.Add(analyzer.Key, analyzer.Value.AsObject());
                    }
                }

                if (o.Analyzers.Count == 0)
                {
                    // When no analyzers are configured, we'll define a default analyzer.
                    o.Analyzers.Add(ElasticsearchConstants.DefaultAnalyzer, new JsonObject
                    {
                        ["type"] = "standard",
                    });
                }
            });

            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<SearchIndexViewModel>();
                o.MemberAccessStrategy.Register<SearchFormViewModel>();
                o.MemberAccessStrategy.Register<SearchResultsViewModel>();
            });

            services.AddElasticServices();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, ElasticSettingsDisplayDriver>();
            services.AddScoped<IDisplayDriver<Query>, ElasticQueryDisplayDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentTypePartIndexSettingsDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, ContentPartFieldIndexSettingsDisplayDriver>();
            services.AddScoped<IAuthorizationHandler, ElasticsearchAuthorizationHandler>();
        }

        private static ConnectionSettings GetConnectionSettings(ElasticConnectionOptions elasticConfiguration)
        {
            var pool = GetConnectionPool(elasticConfiguration);

            var settings = new ConnectionSettings(pool);

            if (elasticConfiguration.ConnectionType != "CloudConnectionPool" && !string.IsNullOrWhiteSpace(elasticConfiguration.Username) && !string.IsNullOrWhiteSpace(elasticConfiguration.Password))
            {
                settings.BasicAuthentication(elasticConfiguration.Username, elasticConfiguration.Password);
            }

            if (!string.IsNullOrWhiteSpace(elasticConfiguration.CertificateFingerprint))
            {
                settings.CertificateFingerprint(elasticConfiguration.CertificateFingerprint);
            }

            if (elasticConfiguration.EnableApiVersioningHeader)
            {
                settings.EnableApiVersioningHeader();
            }

            return settings;
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
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

            routes.MapAreaControllerRoute(
                name: "Elasticsearch.SyncSettings",
                areaName: "OrchardCore.Search.Elasticsearch",
                pattern: _adminOptions.AdminUrlPrefix + "/elasticsearch/SyncSettings",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.SyncSettings) }
            );
        }

        private static bool CheckOptions(ElasticConnectionOptions elasticConnectionOptions, ILogger logger)
        {
            if (elasticConnectionOptions == null)
            {
                logger.LogError("Elasticsearch is enabled but not active because the configuration is missing.");
                return false;
            }

            var optionsAreValid = true;

            if (string.IsNullOrWhiteSpace(elasticConnectionOptions.Url))
            {
                logger.LogError("Elasticsearch is enabled but not active because the 'Url' is missing or empty in application configuration.");
                optionsAreValid = false;
            }

            if (elasticConnectionOptions.Ports?.Length == 0)
            {
                logger.LogError("Elasticsearch is enabled but not active because a port is missing in application configuration.");
                optionsAreValid = false;
            }

            return optionsAreValid;
        }

        private static IConnectionPool GetConnectionPool(ElasticConnectionOptions elasticConfiguration)
        {
            var uris = elasticConfiguration.Ports.Select(port => new Uri($"{elasticConfiguration.Url}:{port}")).Distinct();
            IConnectionPool pool = null;
            switch (elasticConfiguration.ConnectionType)
            {
                case "SingleNodeConnectionPool":
                    pool = new SingleNodeConnectionPool(uris.First());
                    break;

                case "CloudConnectionPool":
                    if (!string.IsNullOrWhiteSpace(elasticConfiguration.Username) && !string.IsNullOrWhiteSpace(elasticConfiguration.Password) && !string.IsNullOrWhiteSpace(elasticConfiguration.CloudId))
                    {
                        var credentials = new BasicAuthenticationCredentials(elasticConfiguration.Username, elasticConfiguration.Password);
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

            return pool;
        }
    }

    [RequireFeatures("OrchardCore.Search")]
    public class SearchStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISearchService, ElasticsearchService>();
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

            services.AddTransient<IDeploymentSource, ElasticIndexRebuildDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ElasticIndexRebuildDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ElasticIndexRebuildDeploymentStepDriver>();

            services.AddTransient<IDeploymentSource, ElasticIndexResetDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ElasticIndexResetDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ElasticIndexResetDeploymentStepDriver>();
        }
    }

    [Feature("OrchardCore.Search.Elasticsearch.Worker")]
    public class ElasticWorkerStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBackgroundTask, IndexingBackgroundTask>();
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
