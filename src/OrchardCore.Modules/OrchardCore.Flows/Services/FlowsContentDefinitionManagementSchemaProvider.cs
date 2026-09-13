using OrchardCore.ContentTypes.Management;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.Services;

internal sealed class FlowsContentDefinitionManagementSchemaProvider : IContentDefinitionManagementSchemaProvider
{
    public IEnumerable<ContentDefinitionManagementSchema> GetSchemas()
    {
        yield return Create<FlowPartSettings>(nameof(FlowPart));
        yield return Create<BagPartSettings>(nameof(BagPart));
    }

    private static ContentDefinitionManagementSchema Create<T>(string appliesTo)
        => new()
        {
            Name = typeof(T).Name,
            Type = typeof(T),
            Scope = ContentDefinitionManagementSchemaScope.ContentTypePart,
            AppliesTo = appliesTo,
        };
}
