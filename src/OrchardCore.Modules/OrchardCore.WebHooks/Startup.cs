using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.WebHooks.Abstractions.Services;
using OrchardCore.WebHooks.Expressions;
using OrchardCore.WebHooks.Handlers;
using OrchardCore.WebHooks.Services;
using OrchardCore.WebHooks.Services.Events;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace OrchardCore.WebHooks
{
    public class Startup : StartupBase
    {
        private const string TimeoutPolicyName = "webhook.timeout";
        private const string RetryPolicyName = "webhook.retry";

        public override void ConfigureServices(IServiceCollection services)
        {
            var registry = services.AddPolicyRegistry();

            ConfigureTimeoutPolicy(registry);
            ConfigureRetryPolicy(registry);

            ConfigureHttpClient(services);

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IContentHandler, ContentsHandler>();
            services.AddScoped<IWebHookManager, WebHookManager>();
            services.AddScoped<IWebHookSender, WebHookSender>();
            services.AddScoped<IWebHookStore, WebHookStore>();
            services.AddScoped<IWebHookEventManager, WebHookEventManagaer>();
            services.AddScoped<IWebHookEventProvider, WildcardWebHookEvent>();
            services.AddScoped<IWebHookEventProvider, ContentWebHookEvents>();
            services.AddScoped<IWebHookExpressionEvaluator, LiquidWebHookExpressionEvaluator>();

            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        private static void ConfigureHttpClient(IServiceCollection services)
        {
            AddWebHookHttpClient(services, "webhooks");
            AddWebHookHttpClient(services, "webhooks_insecure")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                    // TODO: Why won't DangerousAcceptAnyServerCertificateValidator resolve - Need for Linux/MACOS support.  
                    //ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                });
        }

        private static IHttpClientBuilder AddWebHookHttpClient(IServiceCollection services, string clientName)
        {
            return services.AddHttpClient(clientName, c =>
                {
                    c.DefaultRequestHeaders.Add("User-Agent", "Orchard");
                })
                .AddPolicyHandlerFromRegistry(TimeoutPolicyName)
                .AddPolicyHandlerFromRegistry(RetryPolicyName);
        }

        private static void ConfigureTimeoutPolicy(IPolicyRegistry<string> registry)
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            registry.Add(TimeoutPolicyName, timeoutPolicy);
        }

        private static void ConfigureRetryPolicy(IPolicyRegistry<string> registry)
        {
            var retryPolicy = Policy.Handle<HttpRequestException>()
                .OrTransientHttpError()
                .RetryAsync(2);

            registry.Add(RetryPolicyName, retryPolicy);
        }
    }
}
