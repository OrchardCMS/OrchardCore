using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL;

internal sealed class ContainedPartContentItemTypeInitializer : IContentItemTypeInitializer
{
    internal IStringLocalizer S;

    public ContainedPartContentItemTypeInitializer(IStringLocalizer<ContainedPartContentItemTypeInitializer> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public void Initialize(ContentItemType contentItemType, ISchema schema)
    {
        // Get all types with a list part that can contain the current type.
        var parentTypes = schema.AdditionalTypeInstances
            .Where(t => t.Metadata.TryGetValue(nameof(ListPartSettings.ContainedContentTypes), out var containedTypes) && ((containedTypes as IEnumerable<string>)?.Any(ct => ct == contentItemType.Name) ?? false));

        foreach (var parentType in parentTypes)
        {
            var fieldType = schema.AdditionalTypeInstances.FirstOrDefault(t => t is ContainedQueryObjectType);

            if (fieldType == null)
            {
                fieldType = ((IServiceProvider)schema).GetRequiredService<ContainedQueryObjectType>();
                schema.RegisterType(fieldType);
            }

            contentItemType.Field<ContainedQueryObjectType>(parentType.Name.ToFieldName())
                .Description(S["The parent content item of type {0}.", parentType.Name])
                .Type(fieldType)
                .Resolve(context =>
                {
                    return context.Source.ContentItem.As<ContainedPart>();
                });
        }
    }
}
