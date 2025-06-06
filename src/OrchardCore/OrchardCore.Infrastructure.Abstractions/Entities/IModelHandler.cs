namespace OrchardCore.Infrastructure.Entities;

public interface IModelHandler<T>
{
    /// <summary>
    /// This method in invoked during model initializing.
    /// </summary>
    /// <param name="context">An instance of <see cref="InitializingContext{T}"/>.</param>
    Task InitializingAsync(InitializingContext<T> context);

    /// <summary>
    /// This method in invoked after the model was initialized.
    /// </summary>
    /// <param name="context">An instance of <see cref="InitializedContext{T}"/>.</param>
    Task InitializedAsync(InitializedContext<T> context);

    /// <summary>
    /// This method in invoked after the model was loaded from the store.
    /// </summary>
    /// <param name="context">An instance of <see cref="LoadedContext{T}"/>.</param>
    Task LoadedAsync(LoadedContext<T> context);

    /// <summary>
    /// This method in invoked during model validating.
    /// </summary>
    /// <param name="context">An instance of <see cref="ValidatingContext{T}"/>.</param>
    Task ValidatingAsync(ValidatingContext<T> context);

    /// <summary>
    /// This method in invoked after the model was validated.
    /// </summary>
    /// <param name="context">An instance of <see cref="ValidatedContext{T}"/>.</param>
    Task ValidatedAsync(ValidatedContext<T> context);

    /// <summary>
    /// This method in invoked during model removing.
    /// </summary>
    /// <param name="context">An instance of <see cref="DeletingContext{T}"/>.</param>
    Task DeletingAsync(DeletingContext<T> context);

    /// <summary>
    /// This method in invoked after the model was removed.
    /// </summary>
    /// <param name="context">An instance of <see cref="DeletedContext{T}"/>.</param>
    Task DeletedAsync(DeletedContext<T> context);

    /// <summary>
    /// This method in invoked during model updating.
    /// </summary>
    /// <param name="context">An instance of <see cref="UpdatingContext{T}"/>.</param>
    Task UpdatingAsync(UpdatingContext<T> context);

    /// <summary>
    /// This method in invoked after the model was updated.
    /// </summary>
    /// <param name="context">An instance of <see cref="UpdatedContext{T}"/>.</param>
    Task UpdatedAsync(UpdatedContext<T> context);

    /// <summary>
    /// This method in invoked during model saving.
    /// </summary>
    /// <param name="context">An instance of <see cref="CreatingContext{T}"/>.</param>
    Task CreatingAsync(CreatingContext<T> context);

    /// <summary>
    /// This method in invoked after the model was saved.
    /// </summary>
    /// <param name="context">An instance of <see cref="CreatedContext{T}"/>.</param>
    Task CreatedAsync(CreatedContext<T> context);
}
