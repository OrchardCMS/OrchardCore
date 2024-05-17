using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Events;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class DynamicContentFieldsIndexAliasProvider : IIndexAliasProvider, IContentDefinitionEventHandler
{
    private static readonly List<IndexAlias> _aliases = new List<IndexAlias>();

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
    private readonly GraphQLContentOptions _contentOptions;

    public DynamicContentFieldsIndexAliasProvider(IContentDefinitionManager contentDefinitionManager,
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        IOptions<GraphQLContentOptions> contentOptionsAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentFieldProviders = contentFieldProviders;
        _contentOptions = contentOptionsAccessor.Value;
    }

    public IEnumerable<IndexAlias> GetAliases()
    {
        if (_aliases.Count != 0)
        {
            return _aliases;
        }

        var types = _contentDefinitionManager.ListTypeDefinitionsAsync().GetAwaiter().GetResult();
        var parts = types.SelectMany(t => t.Parts);

        foreach (var part in parts)
        {
            foreach (var field in part.PartDefinition.Fields)
            {
                string alias = _contentOptions.ShouldCollapse(part) ?
                    GraphQLContentOptions.GetFieldName(part, part.Name, field.Name) :
                    $"{field.PartDefinition.Name.ToFieldName()}.{field.Name.ToCamelCase()}";

                foreach (var fieldProvider in _contentFieldProviders)
                {
                    if (!fieldProvider.HasFieldIndex(field))
                    {
                        continue;
                    }

                    var (index, indexType) = fieldProvider.GetFieldIndex(field);
                    
                    _aliases.Add(new IndexAlias
                    {
                        Alias = alias,
                        Index = index,
                        IndexType = indexType
                    });

                    break;
                }
            }
        }

        return _aliases;
    }

    private static void ClearAliases()
    {
        _aliases.Clear();
    }

    public void ContentFieldAttached(ContentFieldAttachedContext context) => ClearAliases();

    public void ContentFieldDetached(ContentFieldDetachedContext context) => ClearAliases();

    public void ContentPartAttached(ContentPartAttachedContext context) => ClearAliases();

    public void ContentPartCreated(ContentPartCreatedContext context) => ClearAliases();

    public void ContentPartDetached(ContentPartDetachedContext context) => ClearAliases();

    public void ContentPartImported(ContentPartImportedContext context) => ClearAliases();

    public void ContentPartImporting(ContentPartImportingContext context) { }

    public void ContentPartRemoved(ContentPartRemovedContext context) => ClearAliases();

    public void ContentTypeCreated(ContentTypeCreatedContext context) => ClearAliases();

    public void ContentTypeImported(ContentTypeImportedContext context) => ClearAliases();

    public void ContentTypeImporting(ContentTypeImportingContext context) { }

    public void ContentTypeRemoved(ContentTypeRemovedContext context) => ClearAliases();

    public void ContentTypeUpdated(ContentTypeUpdatedContext context) => ClearAliases();

    public void ContentPartUpdated(ContentPartUpdatedContext context) => ClearAliases();

    public void ContentTypePartUpdated(ContentTypePartUpdatedContext context) => ClearAliases();

    public void ContentFieldUpdated(ContentFieldUpdatedContext context) => ClearAliases();

    public void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context) => ClearAliases();
}
