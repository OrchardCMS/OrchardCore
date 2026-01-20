using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core;

namespace OrchardCore.Indexing.DataMigrations;

internal sealed class WorkerFeatureMigrations : DataMigration
{
#pragma warning disable CA1822 // Member can be static
    public int Create()
#pragma warning restore CA1822
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();

            if (await featuresManager.IsFeatureEnabledAsync(IndexingConstants.Feature.Worker))
            {
                return;
            }

            var shouldEnabled = await featuresManager.IsFeatureEnabledAsync("OrchardCore.Search.Elasticsearch.Worker")
                || await featuresManager.IsFeatureEnabledAsync("OrchardCore.Search.Lucene.Worker");

            if (!shouldEnabled)
            {
                return;
            }

            await featuresManager.EnableFeaturesAsync(IndexingConstants.Feature.Worker);
        });

        return 1;
    }
}
