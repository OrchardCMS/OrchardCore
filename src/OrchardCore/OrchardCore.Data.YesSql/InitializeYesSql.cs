using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Data;

public class InitializeYesSql : ModularTenantEvents
{
    private readonly IStore _store;
    private readonly StoreCollectionOptions _storeCollectionOptions;

    public InitializeYesSql(
        IStore store,
        IOptions<StoreCollectionOptions> storeCollectionOptions)
    {
        _store = store;
        _storeCollectionOptions = storeCollectionOptions.Value;
    }

    public override async Task ActivatingAsync()
    {
        await _store.InitializeAsync();

        foreach (var collection in _storeCollectionOptions.Collections)
        {
            await _store.InitializeCollectionAsync(collection);
        }
    }
}
