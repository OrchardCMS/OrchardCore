namespace OrchardCore.Queries.Core;

public interface IQueryHandler
{
    /// <summary>
    /// This method in invoked during query initializing.
    /// </summary>
    /// <param name="context">An instance of <see cref="InitializingQueryContext"/>.</param>
    Task InitializingAsync(InitializingQueryContext context);

    /// <summary>
    /// This method in invoked after the query was initialized.
    /// </summary>
    /// <param name="context">An instance of <see cref="InitializedQueryContext"/>.</param>
    Task InitializedAsync(InitializedQueryContext context);

    /// <summary>
    /// This method in invoked after the query was loaded from the store.
    /// </summary>
    /// <param name="context">An instance of <see cref="LoadedQueryContext"/>.</param>
    Task LoadedAsync(LoadedQueryContext context);

    /// <summary>
    /// This method in invoked before the query is removed from the store.
    /// </summary>
    /// <param name="context">An instance of <see cref="LoadedQueryContext"/>.</param>
    Task DeletingAsync(DeletingQueryContext context);

    /// <summary>
    /// This method in invoked after the query was removed from the store.
    /// </summary>
    /// <param name="context">An instance of <see cref="DeletedQueryContext"/>.</param>
    Task DeletedAsync(DeletedQueryContext context);

    /// <summary>
    /// This method in invoked before the query is updated.
    /// </summary>
    /// <param name="context">An instance of <see cref="UpdatingQueryContext"/>.</param>
    Task UpdatingAsync(UpdatingQueryContext context);

    /// <summary>
    /// This method in invoked after the query is updated.
    /// </summary>
    /// <param name="context">An instance of <see cref="UpdatedQueryContext"/>.</param>
    Task UpdatedAsync(UpdatedQueryContext context);
}
