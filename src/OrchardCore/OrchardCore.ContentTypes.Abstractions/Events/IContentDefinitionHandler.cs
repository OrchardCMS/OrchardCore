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

    // Content Definition Lifecycle Events

    /// <summary>
    /// Invoked when a content type is created.
    /// </summary>
    /// <param name="context">The context for the content type that was created.</param>
    void ContentTypeCreated(ContentTypeCreatedContext context) { }

    /// <summary>
    /// Invoked when a content type is updated.
    /// </summary>
    /// <param name="context">The context for the content type that was updated.</param>
    void ContentTypeUpdated(ContentTypeUpdatedContext context) { }

    /// <summary>
    /// Invoked when a content type is removed.
    /// </summary>
    /// <param name="context">The context for the content type that was removed.</param>
    void ContentTypeRemoved(ContentTypeRemovedContext context) { }

    /// <summary>
    /// Invoked when a content type is being imported.
    /// </summary>
    /// <param name="context">The context for the content type being imported.</param>
    void ContentTypeImporting(ContentTypeImportingContext context) { }

    /// <summary>
    /// Invoked when a content type has been imported.
    /// </summary>
    /// <param name="context">The context for the content type that was imported.</param>
    void ContentTypeImported(ContentTypeImportedContext context) { }

    /// <summary>
    /// Invoked when a content part is created.
    /// </summary>
    /// <param name="context">The context for the content part that was created.</param>
    void ContentPartCreated(ContentPartCreatedContext context) { }

    /// <summary>
    /// Invoked when a content part is updated.
    /// </summary>
    /// <param name="context">The context for the content part that was updated.</param>
    void ContentPartUpdated(ContentPartUpdatedContext context) { }

    /// <summary>
    /// Invoked when a content part is removed.
    /// </summary>
    /// <param name="context">The context for the content part that was removed.</param>
    void ContentPartRemoved(ContentPartRemovedContext context) { }

    /// <summary>
    /// Invoked when a content part is attached to a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was attached.</param>
    void ContentPartAttached(ContentPartAttachedContext context) { }

    /// <summary>
    /// Invoked when a content part is detached from a content type.
    /// </summary>
    /// <param name="context">The context for the content part that was detached.</param>
    void ContentPartDetached(ContentPartDetachedContext context) { }

    /// <summary>
    /// Invoked when a content part is being imported.
    /// </summary>
    /// <param name="context">The context for the content part being imported.</param>
    void ContentPartImporting(ContentPartImportingContext context) { }

    /// <summary>
    /// Invoked when a content part has been imported.
    /// </summary>
    /// <param name="context">The context for the content part that was imported.</param>
    void ContentPartImported(ContentPartImportedContext context) { }

    /// <summary>
    /// Invoked when a content type part is updated.
    /// </summary>
    /// <param name="context">The context for the content type part that was updated.</param>
    void ContentTypePartUpdated(ContentTypePartUpdatedContext context) { }

    /// <summary>
    /// Invoked when a content field is attached.
    /// </summary>
    /// <param name="context">The context for the content field that was attached.</param>
    void ContentFieldAttached(ContentFieldAttachedContext context) { }

    /// <summary>
    /// Invoked when a content field is updated.
    /// </summary>
    /// <param name="context">The context for the content field that was updated.</param>
    void ContentFieldUpdated(ContentFieldUpdatedContext context) { }

    /// <summary>
    /// Invoked when a content field is detached.
    /// </summary>
    /// <param name="context">The context for the content field that was detached.</param>
    void ContentFieldDetached(ContentFieldDetachedContext context) { }

    /// <summary>
    /// Invoked when a content part field is updated.
    /// </summary>
    /// <param name="context">The context for the content part field that was updated.</param>
    void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context) { }
}

