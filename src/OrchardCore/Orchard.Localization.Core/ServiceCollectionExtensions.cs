using System;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.Localization.PortableObject;

namespace Orchard.Localization
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the services to enable localization using Portable Object files
        /// </summary>
        public static IServiceCollection AddPortableObjectLocalization(this IServiceCollection services, Action<LocalizationOptions> cfg)
        {
            services.AddSingleton<IPluralRuleProvider, DefaultPluralRuleProvider>();
            services.AddSingleton<ITranslationProvider, PoFilesTranslationsProvider>();
            services.AddSingleton<ILocalizationFileLocationProvider, ContentRootPoFileLocationProvider>();
            services.AddSingleton<ILocalizationManager, LocalizationManager>();
            services.AddSingleton<IStringLocalizerFactory, PortableObjectStringLocalizerFactory>();

            services.AddSingleton<IHtmlLocalizerFactory, PortableObjectHtmlLocalizerFactory>();

            services.Configure(cfg);

            return services;
        }
    }
}
