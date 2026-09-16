using OrchardCore.Autoroute.Models;
using OrchardCore.ContentTypes.Management;

namespace OrchardCore.Autoroute.Services;

internal sealed class AutorouteContentDefinitionManagementSchemaProvider : IContentDefinitionManagementSchemaProvider
{
    public IEnumerable<ContentDefinitionManagementSchema> GetSchemas()
    {
        yield return new ContentDefinitionManagementSchema
        {
            Name = nameof(AutoroutePartSettings),
            Type = typeof(AutoroutePartSettings),
            Scope = ContentDefinitionManagementSchemaScope.ContentTypePart,
            AppliesTo = nameof(AutoroutePart),
        };
    }
}
