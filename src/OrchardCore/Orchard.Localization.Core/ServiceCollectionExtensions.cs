using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.Localization.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.Localization.Core
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds tenant level services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOrchardLocalization(this IServiceCollection services)
        {
            services.AddScoped<IPluralRuleProvider, DefaultPluralRuleProvider>();
            services.AddScoped<ITranslationProvider, PoFilesTranslationsProvider>();
            services.AddScoped<ILocalizationManager, LocalizationManager>();
            services.AddScoped<IStringLocalizerFactory, PoStringLocalizerFactory>();

            return services;
        }
    }
}
