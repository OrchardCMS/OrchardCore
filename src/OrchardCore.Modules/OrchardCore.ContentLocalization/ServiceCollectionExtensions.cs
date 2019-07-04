using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.Data.Migration;
using YesSql.Indexes;

namespace OrchardCore.ContentLocalization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentLocalization(this IServiceCollection services)
        {
            services.TryAddScoped<IContentLocalizationManager, DefaultContentLocalizationManager>();
            services.AddSingleton<IIndexProvider, LocalizedContentItemIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentLocalizationHandler, ContentLocalizationPartHandlerCoordinator>();
            services.AddSingleton<ContentPart, LocalizationPart>();

            return services;
        }
    }
}
