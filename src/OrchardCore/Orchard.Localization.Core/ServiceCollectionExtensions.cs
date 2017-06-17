using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.Localization.Abstractions;
using Orchard.Localization.PortableObject;

namespace Orchard.Localization.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOrchardLocalization(this IServiceCollection services, Action<LocalizationOptions> setupAction)
        {
            services.AddSingleton<IPluralRuleProvider, DefaultPluralRuleProvider>();
            services.AddScoped<ITranslationProvider, PoFilesTranslationsProvider>();
            services.AddScoped<ILocalizationFileLocationProvider, DefaultPoFileLocationProvider>();
            services.AddScoped<ILocalizationManager, LocalizationManager>();
            services.AddScoped<IStringLocalizerFactory, PortableObjectStringLocalizerFactory>();

            services.Configure(setupAction);

            return services;
        }
    }
}
