using System.Collections.Generic;

namespace OrchardCore.ContentTransfer;

/// <summary>
/// Resolves import/export handlers for content parts and fields by their type name.
/// Used by <see cref="IContentImportManager"/> to look up the appropriate handlers
/// when iterating over a content type's parts and fields.
/// </summary>
public interface IContentImportHandlerResolver
{
    /// <summary>
    /// Returns the registered field import handlers for the given content field type name.
    /// </summary>
    /// <param name="fieldName">The content field type name (e.g., "TextField", "BooleanField").</param>
    /// <returns>A list of handlers registered for the specified field type.</returns>
    IList<IContentFieldImportHandler> GetFieldHandlers(string fieldName);

    /// <summary>
    /// Returns the registered part import handlers for the given content part type name.
    /// </summary>
    /// <param name="partName">The content part type name (e.g., "TitlePart", "AutoroutePart").</param>
    /// <returns>A list of handlers registered for the specified part type.</returns>
    IList<IContentPartImportHandler> GetPartHandlers(string partName);
}
