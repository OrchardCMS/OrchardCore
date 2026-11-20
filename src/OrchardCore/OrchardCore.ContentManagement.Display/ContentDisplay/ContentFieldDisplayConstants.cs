namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// The names used to display the name of a content field next to its value.
/// </summary>
public static class ContentFieldDisplayConstants
{
    /// <summary>
    /// The wrapper added around the display shape of a field whose name is displayed.
    /// </summary>
    public const string WrapperShapeType = "ContentField_Wrapper";

    /// <summary>
    /// The shape property holding the <see cref="Metadata.Models.ContentPartFieldDefinition"/> of the field
    /// wrapped by <see cref="WrapperShapeType"/>.
    /// </summary>
    public const string PartFieldDefinitionProperty = "ContentFieldWrapperPartFieldDefinition";
}
