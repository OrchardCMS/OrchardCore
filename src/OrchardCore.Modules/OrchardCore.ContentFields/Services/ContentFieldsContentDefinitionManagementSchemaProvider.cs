using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentTypes.Management;

namespace OrchardCore.ContentFields.Services;

internal sealed class ContentFieldsContentDefinitionManagementSchemaProvider : IContentDefinitionManagementSchemaProvider
{
    public IEnumerable<ContentDefinitionManagementSchema> GetSchemas()
    {
        yield return new ContentDefinitionManagementSchema
        {
            Name = nameof(ContentPickerFieldSettings),
            Type = typeof(ContentPickerFieldSettings),
            Scope = ContentDefinitionManagementSchemaScope.ContentPartField,
            AppliesTo = nameof(ContentPickerField),
        };
    }
}
