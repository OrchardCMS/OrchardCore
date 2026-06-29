using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Exporting;

namespace OrchardCore.DataOrchestrator.Helpers;

public static class EtlServiceCollectionExtensions
{
    /// <summary>
    /// Registers an ETL activity and its display driver.
    /// </summary>
    public static IServiceCollection AddEtlActivity<TActivity, TDriver>(this IServiceCollection services)
        where TActivity : class, IEtlActivity
        where TDriver : class, IDisplayDriver<IEtlActivity>
    {
        services.Configure<EtlOptions>(options => options.RegisterActivity(typeof(TActivity), typeof(TDriver)));

        return services;
    }

    /// <summary>
    /// Registers an <see cref="IEtlExportFormat"/> used by file-based ETL destinations.
    /// </summary>
    public static IServiceCollection AddEtlExportFormat<TFormat>(this IServiceCollection services)
        where TFormat : class, IEtlExportFormat
    {
        services.AddScoped<IEtlExportFormat, TFormat>();

        return services;
    }
}
