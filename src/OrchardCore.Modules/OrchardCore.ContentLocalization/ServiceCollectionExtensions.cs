using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentLocalization.Drivers;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using YesSql.Indexes;

namespace OrchardCore.ContentLocalization
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddContentLocalization(this IServiceCollection services)
        {
            services.AddContentPart<LocalizationPart>()
                .UseDisplayDriver<LocalizationPartDisplayDriver>()
                .AddHandler<LocalizationPartHandler>();

            services.TryAddScoped<IContentLocalizationManager, DefaultContentLocalizationManager>();
            services.AddSingleton<IIndexProvider, LocalizedContentItemIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IContentLocalizationHandler, ContentLocalizationPartHandlerCoordinator>();

            return services;
        }
    }
}
