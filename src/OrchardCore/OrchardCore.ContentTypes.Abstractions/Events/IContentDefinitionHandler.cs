namespace OrchardCore.ContentTypes.Events;

/// <summary>
/// Defines methods for handling content definition events during the content type building process.
/// Implementations of this interface allow customization of content types, content parts, and fields 
/// before they are finalized in the content definition.
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
}

