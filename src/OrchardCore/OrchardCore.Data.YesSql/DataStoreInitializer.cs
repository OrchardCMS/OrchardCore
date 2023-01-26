using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Shell;
using OrchardCore.Environment.Shell.Builders;
using YesSql;

namespace OrchardCore.Data;

public class DataStoreInitializer : IShellContextEvents
{
    public async Task CreatedAsync(ShellContext context)
    {
        var store = context.ServiceProvider.GetService<IStore>();
        if (store == null)
        {
            return;
        }

        await store.InitializeAsync();

        var storeCollectionOptions = context.ServiceProvider.GetService<IOptions<StoreCollectionOptions>>().Value;
        foreach (var collection in storeCollectionOptions.Collections)
        {
            await store.InitializeCollectionAsync(collection);
        }
    }
}
