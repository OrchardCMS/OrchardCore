namespace OrchardCore.ContentTypes.Events;

/// <summary>
/// Defines methods for handling content definition events during the content type building process.
/// Implementations of this interface allow customization of content types, content parts, and fields 
/// before they are finalized in the content definition, as well as handling content definition lifecycle events.
/// </summary>
public interface IContentDefinitionHandler
{
    /// <summary>
    /// Invoked during the building of a content type.
    /// Allows modifications or custom logic to be applied to the content type being created.
    /// </summary>
    /// <param name="context">The context for the content type being built.</param>
    void ContentTypeBuilding(ContentTypeBuildingContext context);

    /// <summary>
    /// Invoked during the building of a content part definition.
    /// Allows modification or customization of content parts before they are finalized in the content definition.
    /// </summary>
    /// <param name="context">The context for the content part definition being built.</param>
    void ContentPartBuilding(ContentPartBuildingContext context);

    /// <summary>
    /// Invoked during the building of a part on a content type.
    /// Enables modification or customization of the content part as it is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content type part being built.</param>
    void ContentTypePartBuilding(ContentTypePartBuildingContext context);

    /// <summary>
    /// Invoked during the building of a field on a content part.
    /// Allows customization of fields added to content parts before the final definition is created.
    /// </summary>
    /// <param name="context">The context for the content part field being built.</param>
    void ContentPartFieldBuilding(ContentPartFieldBuildingContext context);

    // Content Definition Lifecycle Events - Async Versions

    /// <summary>
    /// Asynchronously invoked when a content type is created.
    /// </summary>
    /// <param name="context">The context for the content type that was created.</param>
    ValueTask ContentTypeCreatedAsync(ContentTypeCreatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type is updated.
    /// </summary>
    /// <param name="context">The context for the content type that was updated.</param>
    ValueTask ContentTypeUpdatedAsync(ContentTypeUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type is removed.
    /// </summary>
    /// <param name="context">The context for the content type that was removed.</param>
    ValueTask ContentTypeRemovedAsync(ContentTypeRemovedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type is being imported.
    /// </summary>
    /// <param name="context">The context for the content type being imported.</param>
    ValueTask ContentTypeImportingAsync(ContentTypeImportingContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type has been imported.
    /// </summary>
    /// <param name="context">The context for the content type that was imported.</param>
    ValueTask ContentTypeImportedAsync(ContentTypeImportedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is created.
    /// </summary>
    /// <param name="context">The context for the content part that was created.</param>
    ValueTask ContentPartCreatedAsync(ContentPartCreatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is updated.
    /// </summary>
    /// <param name="context">The context for the content part that was updated.</param>
    ValueTask ContentPartUpdatedAsync(ContentPartUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is removed.
    /// </summary>
    /// <param name="context">The context for the content part that was removed.</param>
    ValueTask ContentPartRemovedAsync(ContentPartRemovedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was attached.</param>
    ValueTask ContentPartAttachedAsync(ContentPartAttachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is detached from a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was detached.</param>
    ValueTask ContentPartDetachedAsync(ContentPartDetachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is being imported.
    /// </summary>
    /// <param name="context">The context for the content part being imported.</param>
    ValueTask ContentPartImportingAsync(ContentPartImportingContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part has been imported.
    /// </summary>
    /// <param name="context">The context for the content part that was imported.</param>
    ValueTask ContentPartImportedAsync(ContentPartImportedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type part is updated.
    /// </summary>
    /// <param name="context">The context for the content type part that was updated.</param>
    ValueTask ContentTypePartUpdatedAsync(ContentTypePartUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content field is attached.
    /// </summary>
    /// <param name="context">The context for the content field that was attached.</param>
    ValueTask ContentFieldAttachedAsync(ContentFieldAttachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content field is updated.
    /// </summary>
    /// <param name="context">The context for the content field that was updated.</param>
    ValueTask ContentFieldUpdatedAsync(ContentFieldUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content field is detached.
    /// </summary>
    /// <param name="context">The context for the content field that was detached.</param>
    ValueTask ContentFieldDetachedAsync(ContentFieldDetachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part field is updated.
    /// </summary>
    /// <param name="context">The context for the content part field that was updated.</param>
    ValueTask ContentPartFieldUpdatedAsync(ContentPartFieldUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Invoked when a content type is created.
    /// </summary>
    /// <param name="context">The context for the content type that was created.</param>
    [Obsolete("Use ContentTypeCreatedAsync instead. This method will be removed in a future version.")]
    void ContentTypeCreated(ContentTypeCreatedContext context) 
        => ContentTypeCreatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type is updated.
    /// </summary>
    /// <param name="context">The context for the content type that was updated.</param>
    [Obsolete("Use ContentTypeUpdatedAsync instead. This method will be removed in a future version.")]
    void ContentTypeUpdated(ContentTypeUpdatedContext context) 
        => ContentTypeUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type is removed.
    /// </summary>
    /// <param name="context">The context for the content type that was removed.</param>
    [Obsolete("Use ContentTypeRemovedAsync instead. This method will be removed in a future version.")]
    void ContentTypeRemoved(ContentTypeRemovedContext context) 
        => ContentTypeRemovedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type is being imported.
    /// </summary>
    /// <param name="context">The context for the content type being imported.</param>
    [Obsolete("Use ContentTypeImportingAsync instead. This method will be removed in a future version.")]
    void ContentTypeImporting(ContentTypeImportingContext context) 
        => ContentTypeImportingAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type has been imported.
    /// </summary>
    /// <param name="context">The context for the content type that was imported.</param>
    [Obsolete("Use ContentTypeImportedAsync instead. This method will be removed in a future version.")]
    void ContentTypeImported(ContentTypeImportedContext context) 
        => ContentTypeImportedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is created.
    /// </summary>
    /// <param name="context">The context for the content part that was created.</param>
    [Obsolete("Use ContentPartCreatedAsync instead. This method will be removed in a future version.")]
    void ContentPartCreated(ContentPartCreatedContext context) 
        => ContentPartCreatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is updated.
    /// </summary>
    /// <param name="context">The context for the content part that was updated.</param>
    [Obsolete("Use ContentPartUpdatedAsync instead. This method will be removed in a future version.")]
    void ContentPartUpdated(ContentPartUpdatedContext context) 
        => ContentPartUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is removed.
    /// </summary>
    /// <param name="context">The context for the content part that was removed.</param>
    [Obsolete("Use ContentPartRemovedAsync instead. This method will be removed in a future version.")]
    void ContentPartRemoved(ContentPartRemovedContext context) 
        => ContentPartRemovedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was attached.</param>
    [Obsolete("Use ContentPartAttachedAsync instead. This method will be removed in a future version.")]
    void ContentPartAttached(ContentPartAttachedContext context) 
        => ContentPartAttachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is detached from a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was detached.</param>
    [Obsolete("Use ContentPartDetachedAsync instead. This method will be removed in a future version.")]
    void ContentPartDetached(ContentPartDetachedContext context) 
        => ContentPartDetachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is being imported.
    /// </summary>
    /// <param name="context">The context for the content part being imported.</param>
    [Obsolete("Use ContentPartImportingAsync instead. This method will be removed in a future version.")]
    void ContentPartImporting(ContentPartImportingContext context) 
        => ContentPartImportingAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part has been imported.
    /// </summary>
    /// <param name="context">The context for the content part that was imported.</param>
    [Obsolete("Use ContentPartImportedAsync instead. This method will be removed in a future version.")]
    void ContentPartImported(ContentPartImportedContext context) 
        => ContentPartImportedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type part is updated.
    /// </summary>
    /// <param name="context">The context for the content type part that was updated.</param>
    [Obsolete("Use ContentTypePartUpdatedAsync instead. This method will be removed in a future version.")]
    void ContentTypePartUpdated(ContentTypePartUpdatedContext context) 
        => ContentTypePartUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content field is attached.
    /// </summary>
    /// <param name="context">The context for the content field that was attached.</param>
    [Obsolete("Use ContentFieldAttachedAsync instead. This method will be removed in a future version.")]
    void ContentFieldAttached(ContentFieldAttachedContext context) 
        => ContentFieldAttachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content field is updated.
    /// </summary>
    /// <param name="context">The context for the content field that was updated.</param>
    [Obsolete("Use ContentFieldUpdatedAsync instead. This method will be removed in a future version.")]
    void ContentFieldUpdated(ContentFieldUpdatedContext context) 
        => ContentFieldUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content field is detached.
    /// </summary>
    /// <param name="context">The context for the content field that was detached.</param>
    [Obsolete("Use ContentFieldDetachedAsync instead. This method will be removed in a future version.")]
    void ContentFieldDetached(ContentFieldDetachedContext context) 
        => ContentFieldDetachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part field is updated.
    /// </summary>
    /// <param name="context">The context for the content part field that was updated.</param>
    [Obsolete("Use ContentPartFieldUpdatedAsync instead. This method will be removed in a future version.")]
    void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context) 
        => ContentPartFieldUpdatedAsync(context).GetAwaiter().GetResult();
}

/// <summary>
/// Provides a base implementation of <see cref="IContentDefinitionHandler"/> with virtual methods that have default implementations.
/// Implementations should inherit from this class and override only the methods they need to customize.
/// </summary>
public class ContentDefinitionHandler : IContentDefinitionHandler
{
    /// <summary>
    /// Invoked during the building of a content type.
    /// Allows modifications or custom logic to be applied to the content type being created.
    /// </summary>
    /// <param name="context">The context for the content type being built.</param>
    public virtual void ContentTypeBuilding(ContentTypeBuildingContext context)
    {
    }

    /// <summary>
    /// Invoked during the building of a content part definition.
    /// Allows modification or customization of content parts before they are finalized in the content definition.
    /// </summary>
    /// <param name="context">The context for the content part definition being built.</param>
    public virtual void ContentPartBuilding(ContentPartBuildingContext context)
    {
    }

    /// <summary>
    /// Invoked during the building of a part on a content type.
    /// Enables modification or customization of the content part as it is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content type part being built.</param>
    public virtual void ContentTypePartBuilding(ContentTypePartBuildingContext context)
    {
    }

    /// <summary>
    /// Invoked during the building of a field on a content part.
    /// Allows customization of fields added to content parts before the final definition is created.
    /// </summary>
    /// <param name="context">The context for the content part field being built.</param>
    public virtual void ContentPartFieldBuilding(ContentPartFieldBuildingContext context)
    {
    }

    // Content Definition Lifecycle Events - Async Versions

    /// <summary>
    /// Asynchronously invoked when a content type is created.
    /// </summary>
    /// <param name="context">The context for the content type that was created.</param>
    public virtual ValueTask ContentTypeCreatedAsync(ContentTypeCreatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type is updated.
    /// </summary>
    /// <param name="context">The context for the content type that was updated.</param>
    public virtual ValueTask ContentTypeUpdatedAsync(ContentTypeUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type is removed.
    /// </summary>
    /// <param name="context">The context for the content type that was removed.</param>
    public virtual ValueTask ContentTypeRemovedAsync(ContentTypeRemovedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type is being imported.
    /// </summary>
    /// <param name="context">The context for the content type being imported.</param>
    public virtual ValueTask ContentTypeImportingAsync(ContentTypeImportingContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type has been imported.
    /// </summary>
    /// <param name="context">The context for the content type that was imported.</param>
    public virtual ValueTask ContentTypeImportedAsync(ContentTypeImportedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is created.
    /// </summary>
    /// <param name="context">The context for the content part that was created.</param>
    public virtual ValueTask ContentPartCreatedAsync(ContentPartCreatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is updated.
    /// </summary>
    /// <param name="context">The context for the content part that was updated.</param>
    public virtual ValueTask ContentPartUpdatedAsync(ContentPartUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is removed.
    /// </summary>
    /// <param name="context">The context for the content part that was removed.</param>
    public virtual ValueTask ContentPartRemovedAsync(ContentPartRemovedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was attached.</param>
    public virtual ValueTask ContentPartAttachedAsync(ContentPartAttachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is detached from a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was detached.</param>
    public virtual ValueTask ContentPartDetachedAsync(ContentPartDetachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part is being imported.
    /// </summary>
    /// <param name="context">The context for the content part being imported.</param>
    public virtual ValueTask ContentPartImportingAsync(ContentPartImportingContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part has been imported.
    /// </summary>
    /// <param name="context">The context for the content part that was imported.</param>
    public virtual ValueTask ContentPartImportedAsync(ContentPartImportedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content type part is updated.
    /// </summary>
    /// <param name="context">The context for the content type part that was updated.</param>
    public virtual ValueTask ContentTypePartUpdatedAsync(ContentTypePartUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content field is attached.
    /// </summary>
    /// <param name="context">The context for the content field that was attached.</param>
    public virtual ValueTask ContentFieldAttachedAsync(ContentFieldAttachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content field is updated.
    /// </summary>
    /// <param name="context">The context for the content field that was updated.</param>
    public virtual ValueTask ContentFieldUpdatedAsync(ContentFieldUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content field is detached.
    /// </summary>
    /// <param name="context">The context for the content field that was detached.</param>
    public virtual ValueTask ContentFieldDetachedAsync(ContentFieldDetachedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Asynchronously invoked when a content part field is updated.
    /// </summary>
    /// <param name="context">The context for the content part field that was updated.</param>
    public virtual ValueTask ContentPartFieldUpdatedAsync(ContentPartFieldUpdatedContext context) 
        => ValueTask.CompletedTask;

    /// <summary>
    /// Invoked when a content type is created.
    /// </summary>
    /// <param name="context">The context for the content type that was created.</param>
    [Obsolete("Use ContentTypeCreatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentTypeCreated(ContentTypeCreatedContext context) 
        => ContentTypeCreatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type is updated.
    /// </summary>
    /// <param name="context">The context for the content type that was updated.</param>
    [Obsolete("Use ContentTypeUpdatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentTypeUpdated(ContentTypeUpdatedContext context) 
        => ContentTypeUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type is removed.
    /// </summary>
    /// <param name="context">The context for the content type that was removed.</param>
    [Obsolete("Use ContentTypeRemovedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentTypeRemoved(ContentTypeRemovedContext context) 
        => ContentTypeRemovedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type is being imported.
    /// </summary>
    /// <param name="context">The context for the content type being imported.</param>
    [Obsolete("Use ContentTypeImportingAsync instead. This method will be removed in a future version.")]
    public virtual void ContentTypeImporting(ContentTypeImportingContext context) 
        => ContentTypeImportingAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type has been imported.
    /// </summary>
    /// <param name="context">The context for the content type that was imported.</param>
    [Obsolete("Use ContentTypeImportedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentTypeImported(ContentTypeImportedContext context) 
        => ContentTypeImportedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is created.
    /// </summary>
    /// <param name="context">The context for the content part that was created.</param>
    [Obsolete("Use ContentPartCreatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartCreated(ContentPartCreatedContext context) 
        => ContentPartCreatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is updated.
    /// </summary>
    /// <param name="context">The context for the content part that was updated.</param>
    [Obsolete("Use ContentPartUpdatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartUpdated(ContentPartUpdatedContext context) 
        => ContentPartUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is removed.
    /// </summary>
    /// <param name="context">The context for the content part that was removed.</param>
    [Obsolete("Use ContentPartRemovedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartRemoved(ContentPartRemovedContext context) 
        => ContentPartRemovedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was attached.</param>
    [Obsolete("Use ContentPartAttachedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartAttached(ContentPartAttachedContext context) 
        => ContentPartAttachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is detached from a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was detached.</param>
    [Obsolete("Use ContentPartDetachedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartDetached(ContentPartDetachedContext context) 
        => ContentPartDetachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part is being imported.
    /// </summary>
    /// <param name="context">The context for the content part being imported.</param>
    [Obsolete("Use ContentPartImportingAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartImporting(ContentPartImportingContext context) 
        => ContentPartImportingAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part has been imported.
    /// </summary>
    /// <param name="context">The context for the content part that was imported.</param>
    [Obsolete("Use ContentPartImportedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartImported(ContentPartImportedContext context) 
        => ContentPartImportedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content type part is updated.
    /// </summary>
    /// <param name="context">The context for the content type part that was updated.</param>
    [Obsolete("Use ContentTypePartUpdatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentTypePartUpdated(ContentTypePartUpdatedContext context) 
        => ContentTypePartUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content field is attached.
    /// </summary>
    /// <param name="context">The context for the content field that was attached.</param>
    [Obsolete("Use ContentFieldAttachedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentFieldAttached(ContentFieldAttachedContext context) 
        => ContentFieldAttachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content field is updated.
    /// </summary>
    /// <param name="context">The context for the content field that was updated.</param>
    [Obsolete("Use ContentFieldUpdatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentFieldUpdated(ContentFieldUpdatedContext context) 
        => ContentFieldUpdatedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content field is detached.
    /// </summary>
    /// <param name="context">The context for the content field that was detached.</param>
    [Obsolete("Use ContentFieldDetachedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentFieldDetached(ContentFieldDetachedContext context) 
        => ContentFieldDetachedAsync(context).GetAwaiter().GetResult();

    /// <summary>
    /// Invoked when a content part field is updated.
    /// </summary>
    /// <param name="context">The context for the content part field that was updated.</param>
    [Obsolete("Use ContentPartFieldUpdatedAsync instead. This method will be removed in a future version.")]
    public virtual void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context) 
        => ContentPartFieldUpdatedAsync(context).GetAwaiter().GetResult();
}

