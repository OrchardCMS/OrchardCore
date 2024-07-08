using System.Threading.Tasks;

namespace OrchardCore.Queries.Core;

public interface IQueryHandler
{
    Task InitializingAsync(InitializingQueryContext context);

    Task InitializedAsync(InitializedQueryContext context);

    Task LoadedAsync(LoadedQueryContext context);

    Task DeletingAsync(DeletingQueryContext context);

    Task DeletedAsync(DeletedQueryContext context);

    Task UpdatingAsync(UpdatingQueryContext context);

    Task UpdatedAsync(UpdatedQueryContext context);
}
