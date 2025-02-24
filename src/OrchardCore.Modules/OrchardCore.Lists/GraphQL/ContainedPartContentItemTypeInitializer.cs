using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL;

internal sealed class ContainedPartContentItemTypeInitializer : IContentItemTypeInitializer
{
    internal readonly IStringLocalizer S;

    public ContainedPartContentItemTypeInitializer(IStringLocalizer<ContainedPartContentItemTypeInitializer> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public void Initialize(ContentItemType contentItemType, ISchema schema)
    {
        foreach (var type in schema.AdditionalTypeInstances)
        {
            // Get all types with a list part that can contain the current type.
            if (!type.Metadata.TryGetValue(nameof(ListPartSettings.ContainedContentTypes), out var containedTypes))
            {
                continue;
            }

            if ((containedTypes as IEnumerable<string>)?.Any(ct => ct == contentItemType.Name) != true)
            {
                continue;
            }

            var fieldType = schema.AdditionalTypeInstances.FirstOrDefault(t => t is ContainedQueryObjectType);

            if (fieldType == null)
            {
                fieldType = ((IServiceProvider)schema).GetRequiredService<ContainedQueryObjectType>();
                schema.RegisterType(fieldType);
            }

            contentItemType.Field<ContainedQueryObjectType>(type.Name.ToFieldName())
                .Description(S["The parent content item of type {0}.", type.Name])
                .Type(fieldType)
                .Resolve(context =>
                {
                    return context.Source.ContentItem.As<ContainedPart>();
                });
        }
    }
}
