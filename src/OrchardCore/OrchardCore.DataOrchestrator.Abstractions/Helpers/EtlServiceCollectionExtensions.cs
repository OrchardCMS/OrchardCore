using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DataOrchestrator.Activities;

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
}
