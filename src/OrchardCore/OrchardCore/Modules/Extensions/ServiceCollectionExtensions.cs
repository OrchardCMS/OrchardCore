using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using OrchardCore;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Localization;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Modules.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Routing singleton and global config types used to isolate tenants from the host.
    /// </summary>
    private static readonly Type[] _routingTypesToIsolate = new ServiceCollection()
        .AddRouting()
        .Where(sd =>
            sd.Lifetime == ServiceLifetime.Singleton ||
            sd.ServiceType == typeof(IConfigureOptions<RouteOptions>))
        .Select(sd => sd.GetImplementationType())
        .ToArray();

    /// <summary>
    /// Http client singleton types used to isolate tenants from the host.
    /// </summary>
    private static readonly Type[] _httpClientTypesToIsolate = new ServiceCollection()
        .AddHttpClient()
        .Where(sd => sd.Lifetime == ServiceLifetime.Singleton)
        .Select(sd => sd.GetImplementationType())
        .Except(new ServiceCollection()
            .AddLogging()
            .Where(sd => sd.Lifetime == ServiceLifetime.Singleton)
            .Select(sd => sd.GetImplementationType()))
        .ToArray();

    /// <summary>
    /// Metrics singletons used to isolate tenants from the host.
    /// </summary>
    private static readonly Type[] _metricsTypesToIsolate = new ServiceCollection()
        .AddMetrics()
        .Where(sd => sd.Lifetime == ServiceLifetime.Singleton)
        .Select(sd => sd.GetImplementationType())
        .ToArray();

    /// <summary>
    /// Adds OrchardCore services to the host service collection.
    /// </summary>
    public static OrchardCoreBuilder AddOrchardCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // If an instance of OrchardCoreBuilder exists reuse it,
        // so we can call AddOrchardCore several times.
        var builder = services
            .LastOrDefault(d => d.ServiceType == typeof(OrchardCoreBuilder))?
            .ImplementationInstance as OrchardCoreBuilder;

        if (builder == null)
        {
            builder = new OrchardCoreBuilder(services);
            services.AddSingleton(builder);

            AddDefaultServices(builder);
            AddShellServices(builder);
            AddExtensionServices(builder);
            AddStaticFiles(builder);

            AddMetrics(builder);
            AddRouting(builder);
            IsolateHttpClient(builder);
            AddEndpointsApiExplorer(builder);
            AddAntiForgery(builder);
            AddSameSiteCookieBackwardsCompatibility(builder);
            AddAuthentication(builder);
            AddDataProtection(builder);

            // Register the list of services to be resolved later on
            services.AddSingleton(services);
        }

        return builder;
    }

    /// <summary>
    /// Adds OrchardCore services to the host service collection and let the app change
    /// the default behavior and set of features through a configure action.
    /// </summary>
    public static IServiceCollection AddOrchardCore(this IServiceCollection services, Action<OrchardCoreBuilder> configure)
    {
        var builder = services.AddOrchardCore();

        configure?.Invoke(builder);

        return services;
    }

    private static void AddDefaultServices(OrchardCoreBuilder builder)
    {
        var services = builder.ApplicationServices;

        services.AddLogging();
        services.AddOptions();

        // These services might be moved at a higher level if no components from OrchardCore needs them.
        services.AddLocalization();

        // For performance, prevents the 'ResourceManagerStringLocalizer' from being used.
        // Also support pluralization.
        services.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
        services.AddSingleton<IHtmlLocalizerFactory, NullHtmlLocalizerFactory>();

        services.AddWebEncoders();

        services.AddHttpContextAccessor();
        services.AddSingleton<IClock, Clock>();
        services.AddScoped<ILocalClock, LocalClock>();

        services.AddScoped<ILocalizationService, DefaultLocalizationService>();
        services.AddScoped<ICalendarManager, DefaultCalendarManager>();
        services.AddScoped<ICalendarSelector, DefaultCalendarSelector>();

        services.AddSingleton<IPoweredByMiddlewareOptions, PoweredByMiddlewareOptions>();
        services.AddTransient<IConfigureOptions<DocumentJsonSerializerOptions>, DocumentJsonSerializerOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<MvcOptions>, MvcOptionsConfiguration>();

        services.AddScoped<IOrchardHelper, DefaultOrchardHelper>();
        services.AddSingleton<IClientIPAddressAccessor, DefaultClientIPAddressAccessor>();
        services.AddSingleton<ISlugService, SlugService>();

        builder.ConfigureServices((services, serviceProvider) =>
        {
            services.AddSingleton<LocalLock>();
            services.AddSingleton<ILocalLock>(sp => sp.GetRequiredService<LocalLock>());
            services.AddSingleton<IDistributedLock>(sp => sp.GetRequiredService<LocalLock>());

            var configuration = serviceProvider.GetService<IShellConfiguration>();

            services.Configure<CultureOptions>(configuration.GetSection("OrchardCore_Localization_CultureOptions"));
        });
    }

    private static void AddShellServices(OrchardCoreBuilder builder)
    {
        var services = builder.ApplicationServices;

        // Use a single tenant and all features by default
        services.AddHostingShellServices();
        services.AddAllFeaturesDescriptor();

        // Registers the application primary feature.
        services.AddTransient(sp => new ShellFeature
        (
            sp.GetRequiredService<IHostEnvironment>().ApplicationName, alwaysEnabled: true)
        );

        // Registers the application default feature.
        services.AddTransient(sp => new ShellFeature
        (
            Application.DefaultFeatureId, alwaysEnabled: true)
        );

        builder.ConfigureServices(shellServices =>
        {
            shellServices.AddScoped<IShellReleaseManager, DefaultShellReleaseManager>();
            shellServices.AddTransient<IConfigureOptions<ShellContextOptions>, ShellContextOptionsSetup>();
            shellServices.AddNullFeatureProfilesService();
            shellServices.AddFeatureValidation();
            shellServices.ConfigureFeatureProfilesRuleOptions();
        });
    }

    private static void AddExtensionServices(OrchardCoreBuilder builder)
    {
        builder.ApplicationServices.AddSingleton<IModuleNamesProvider, AssemblyAttributeModuleNamesProvider>();
        builder.ApplicationServices.AddSingleton<IApplicationContext, ModularApplicationContext>();

        builder.ApplicationServices.AddExtensionManagerHost();

        builder.ConfigureServices(services =>
        {
            services.AddExtensionManager();
            services.AddScoped<IShellFeaturesManager, ShellFeaturesManager>();
            services.AddScoped<IShellDescriptorFeaturesManager, ShellDescriptorFeaturesManager>();
        });
    }

    /// <summary>
    /// Adds tenant level configuration to serve static files from modules.
    /// </summary>
    private static void AddStaticFiles(OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IModuleStaticFileProvider>(serviceProvider =>
            {
                var env = serviceProvider.GetRequiredService<IHostEnvironment>();
                var appContext = serviceProvider.GetRequiredService<IApplicationContext>();

                IModuleStaticFileProvider fileProvider;
                if (env.IsDevelopment())
                {
                    var fileProviders = new List<IStaticFileProvider>
                    {
                        new ModuleProjectStaticFileProvider(appContext),
                        new ModuleEmbeddedStaticFileProvider(appContext)
                    };
                    fileProvider = new ModuleCompositeStaticFileProvider(fileProviders);
                }
                else
                {
                    fileProvider = new ModuleEmbeddedStaticFileProvider(appContext);
                }
                return fileProvider;
            });

            services.AddSingleton<IStaticFileProvider>(serviceProvider =>
            {
                return serviceProvider.GetRequiredService<IModuleStaticFileProvider>();
            });
        });

        builder.Configure((app, routes, serviceProvider) =>
        {
            var fileProvider = serviceProvider.GetRequiredService<IModuleStaticFileProvider>();

            var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>();
            // Cache static files for a year as they are coming from embedded resources and should not vary.
            var cacheControl = shellConfiguration.GetValue("StaticFileOptions:CacheControl", $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}, s-maxage={TimeSpan.FromDays(365.25).TotalSeconds}");

            // Use the current options values but without mutating the resolved instance.
            var options = serviceProvider.GetRequiredService<IOptions<StaticFileOptions>>().Value;
            options = new StaticFileOptions
            {
                RequestPath = string.Empty,
                FileProvider = fileProvider,
                RedirectToAppendTrailingSlash = options.RedirectToAppendTrailingSlash,
                ContentTypeProvider = options.ContentTypeProvider,
                DefaultContentType = options.DefaultContentType,
                ServeUnknownFileTypes = options.ServeUnknownFileTypes,
                HttpsCompression = options.HttpsCompression,

                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = cacheControl;
                },
            };

            app.UseStaticFiles(options);
        });
    }

    /// <summary>
    /// Adds isolated tenant level metrics services.
    /// </summary>
    private static void AddMetrics(OrchardCoreBuilder builder)
    {
        // 'AddMetrics()' is called by the host.

        builder.ConfigureServices(collection =>
        {
            // The 'DefaultMeterFactory' caches 'Meters' in a non thread safe dictionary.
            // So, we need to register an isolated 'IMeterFactory' singleton per tenant.
            var descriptorsToRemove = collection
                .Where(sd =>
                    sd is ClonedSingletonDescriptor &&
                    _metricsTypesToIsolate.Contains(sd.GetImplementationType()))
                .ToArray();
            // Isolate each tenant from the host.

            foreach (var descriptor in descriptorsToRemove)
            {
                collection.Remove(descriptor);
            }

            collection.AddMetrics();
        },
        order: OrchardCoreConstants.ConfigureOrder.InfrastructureService);
    }

    /// <summary>
    /// Adds isolated tenant level routing services.
    /// </summary>
    private static void AddRouting(OrchardCoreBuilder builder)
    {
        // 'AddRouting()' is called by the host.

        builder.ConfigureServices(collection =>
        {
            // The routing system is not tenant aware and uses a global list of endpoint data sources which is
            // setup by the default configuration of 'RouteOptions' and mutated on each call of 'UseEndPoints()'.
            // So, we need isolated routing singletons (and a default configuration) per tenant.

            var descriptorsToRemove = collection
                .Where(sd =>
                    (sd is ClonedSingletonDescriptor ||
                    sd.ServiceType == typeof(IConfigureOptions<RouteOptions>)) &&
                    _routingTypesToIsolate.Contains(sd.GetImplementationType()))
                .ToArray();

            // Isolate each tenant from the host.
            foreach (var descriptor in descriptorsToRemove)
            {
                collection.Remove(descriptor);
            }

            collection.AddRouting();
        },
        order: OrchardCoreConstants.ConfigureOrder.InfrastructureService);
    }

    /// <summary>
    /// Isolates tenant http client singletons and configurations from the host.
    /// </summary>
    private static void IsolateHttpClient(OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(collection =>
        {
            // Each tenant needs isolated http client singletons and configurations, so that
            // typed clients/handlers are activated/resolved from the right tenant container.

            // Retrieve current options configurations.
            var configurationDescriptorsToRemove = collection
                .Where(sd =>
                    sd.ServiceType.IsGenericType &&
                    sd.ServiceType.GenericTypeArguments.Contains(typeof(HttpClientFactoryOptions)))
                .ToArray();

            // Retrieve all descriptors to remove.
            var descriptorsToRemove = collection
                .Where(sd =>
                    sd is ClonedSingletonDescriptor &&
                    _httpClientTypesToIsolate.Contains(sd.GetImplementationType()))
                .Concat(configurationDescriptorsToRemove)
                .ToArray();

            // Isolate each tenant from the host.
            foreach (var descriptor in descriptorsToRemove)
            {
                collection.Remove(descriptor);
            }

            // Make the http client factory 'IDisposable'.
            collection.AddSingleton<TenantHttpClientFactory>();
            collection.AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<TenantHttpClientFactory>());
            collection.AddSingleton<IHttpMessageHandlerFactory>(sp => sp.GetRequiredService<TenantHttpClientFactory>());
        },
        order: OrchardCoreConstants.ConfigureOrder.InfrastructureService);
    }

    /// <summary>
    /// Configures ApiExplorer at the tenant level using <see cref="Endpoint.Metadata"/>.
    /// </summary>
    private static void AddEndpointsApiExplorer(OrchardCoreBuilder builder)
    {
        // 'AddEndpointsApiExplorer()' is called by the host.

        builder.ConfigureServices(collection =>
        {
            // Remove the related host singletons as they are not tenant aware.
            var descriptorsToRemove = collection
                .Where(sd =>
                    sd is ClonedSingletonDescriptor &&
                    (sd.ServiceType == typeof(IActionDescriptorCollectionProvider) ||
                    sd.ServiceType == typeof(IApiDescriptionGroupCollectionProvider)))
                .ToArray();

            // Isolate each tenant from the host.
            foreach (var descriptor in descriptorsToRemove)
            {
                collection.Remove(descriptor);
            }

            // Configure ApiExplorer at the tenant level.
            collection.AddEndpointsApiExplorer();
        },
        order: OrchardCoreConstants.ConfigureOrder.InfrastructureService);
    }

    /// <summary>
    /// Adds host and tenant level antiforgery services.
    /// </summary>
    private static void AddAntiForgery(OrchardCoreBuilder builder)
    {
        builder.ApplicationServices.AddAntiforgery();

        builder.ConfigureServices((services, serviceProvider) =>
        {
            var settings = serviceProvider.GetRequiredService<ShellSettings>();
            var cookieName = "__orchantiforgery_" + settings.VersionId;

            // Re-register the antiforgery services to be tenant-aware.
            var collection = new ServiceCollection()
                .AddAntiforgery(options =>
                {
                    options.Cookie.Name = cookieName;

                    // Don't set the cookie builder 'Path' so that it uses the 'IAuthenticationFeature' value
                    // set by the pipeline and coming from the request 'PathBase' which already ends with the
                    // tenant prefix but may also start by a path related e.g to a virtual folder.
                });

            services.Add(collection);
        });
    }

    /// <summary>
    /// Adds backwards compatibility to the handling of SameSite cookies.
    /// </summary>
    private static void AddSameSiteCookieBackwardsCompatibility(OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext => CheckSameSiteBackwardsCompatibility(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => CheckSameSiteBackwardsCompatibility(cookieContext.Context, cookieContext.CookieOptions);
            });
        })
        .Configure(app =>
        {
            app.UseCookiePolicy();
        });
    }

    private static void CheckSameSiteBackwardsCompatibility(HttpContext httpContext, CookieOptions options)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        if (options.SameSite == SameSiteMode.None)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                return;
            }

            // Cover all iOS based browsers here. This includes:
            // - Safari on iOS 12 for iPhone, iPod Touch, iPad.
            // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad.
            // - Chrome on iOS 12 for iPhone, iPod Touch, iPad.
            // All of which are broken by SameSite=None, because they use the iOS networking stack.
            if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
            {
                options.SameSite = SameSiteMode.Unspecified;
                return;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
            // - Safari on Mac OS X.
            // This does not include:
            // - Chrome on Mac OS X.
            // Because they do not use the Mac OS networking stack.
            if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
                userAgent.Contains("Version/") && userAgent.Contains("Safari"))
            {
                options.SameSite = SameSiteMode.Unspecified;
                return;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None,
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions,
            // but pre-Chromium Edge does not require SameSite=None.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                options.SameSite = SameSiteMode.Unspecified;
            }
        }
    }

    /// <summary>
    /// Adds host and tenant level authentication services and configuration.
    /// </summary>
    private static void AddAuthentication(OrchardCoreBuilder builder)
    {
        builder.ApplicationServices.AddAuthentication();

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication();

            // IAuthenticationSchemeProvider is already registered at the host level.
            // We need to register it again so it is taken into account at the tenant level
            // because it holds a reference to an underlying dictionary, responsible of storing
            // the registered schemes which need to be distinct for each tenant.
            services.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
        })
        .Configure(app =>
        {
            app.UseAuthentication();
        }, order: OrchardCoreConstants.ConfigureOrder.Authentication);
    }

    /// <summary>
    /// Adds tenant level data protection services.
    /// </summary>
    private static void AddDataProtection(OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((services, serviceProvider) =>
        {
            var settings = serviceProvider.GetRequiredService<ShellSettings>();
            var options = serviceProvider.GetRequiredService<IOptions<ShellOptions>>();

            // The 'FileSystemXmlRepository' will create the directory, but only if it is not overridden.
            var directory = new DirectoryInfo(Path.Combine(
                options.Value.ShellsApplicationDataPath,
                options.Value.ShellsContainerName,
                settings.Name, "DataProtection-Keys"));

            // Re-register the data protection services to be tenant-aware so that modules that internally
            // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
            // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
            // By default, the key ring is stored in the tenant directory of the configured App_Data path.
            var collection = new ServiceCollection()
                .AddDataProtection()
                .PersistKeysToFileSystem(directory)
                .SetApplicationName(settings.Name)
                .AddKeyManagementOptions(o => o.XmlEncryptor ??= new NullXmlEncryptor())
                .Services;

            // Remove any previously registered options setups.
            services.RemoveAll<IConfigureOptions<KeyManagementOptions>>();
            services.RemoveAll<IConfigureOptions<DataProtectionOptions>>();

            services.Add(collection);
        }, order: OrchardCoreConstants.ConfigureOrder.DataProtection);
    }
}
