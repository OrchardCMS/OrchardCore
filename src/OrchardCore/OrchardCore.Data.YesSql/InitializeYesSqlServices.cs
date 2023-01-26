using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Shell;
using OrchardCore.Environment.Shell.Builders;
using YesSql;

namespace OrchardCore.Data;

public class InitializeYesSqlServices : IShellContextEvents
{
    private readonly IStore _store;
    private readonly StoreCollectionOptions _storeCollectionOptions;

    public InitializeYesSqlServices(
        IStore store,
        IOptions<StoreCollectionOptions> storeCollectionOptions)
    {
        _store = store;
        _storeCollectionOptions = storeCollectionOptions.Value;
    }

    public async Task CreatedAsync(ShellContext context)
    {
        await _store.InitializeAsync();

        foreach (var collection in _storeCollectionOptions.Collections)
        {
            await _store.InitializeCollectionAsync(collection);
        }
    }
}
