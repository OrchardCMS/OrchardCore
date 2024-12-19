namespace OrchardCore.Queries.Core;

public abstract class QueryHandlerBase : IQueryHandler
{
    public virtual Task DeletingAsync(DeletingQueryContext context)
        => Task.CompletedTask;

    public virtual Task DeletedAsync(DeletedQueryContext context)
        => Task.CompletedTask;

    public virtual Task InitializingAsync(InitializingQueryContext context)
        => Task.CompletedTask;

    public virtual Task InitializedAsync(InitializedQueryContext context)
        => Task.CompletedTask;

    public virtual Task LoadedAsync(LoadedQueryContext context)
        => Task.CompletedTask;

    public virtual Task UpdatingAsync(UpdatingQueryContext context)
        => Task.CompletedTask;

    public virtual Task UpdatedAsync(UpdatedQueryContext context)
        => Task.CompletedTask;
}
