using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Environment.Shell;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class DynamicContentFieldsIndexAliasProvider : IIndexAliasProvider, IContentDefinitionEventHandler
{
    private static readonly ConcurrentDictionary<string, List<IndexAlias>> _aliases = new ConcurrentDictionary<string, List<IndexAlias>>();

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
    private readonly ShellSettings _shellSettings;
    private readonly GraphQLContentOptions _contentOptions;

    public DynamicContentFieldsIndexAliasProvider(IContentDefinitionManager contentDefinitionManager,
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        IOptions<GraphQLContentOptions> contentOptionsAccessor,
        ShellSettings shellSettings)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentFieldProviders = contentFieldProviders;
        _contentOptions = contentOptionsAccessor.Value;
        _shellSettings = shellSettings;
    }

    public IEnumerable<IndexAlias> GetAliases()
    {
        var tenantAliases = _aliases.GetOrAdd(_shellSettings.Name, _ => []);

        if (tenantAliases.Count != 0)
        {
            return tenantAliases;
        }

        var types = _contentDefinitionManager.ListTypeDefinitionsAsync().GetAwaiter().GetResult();
        var parts = types.SelectMany(t => t.Parts);

        foreach (var part in parts)
        {
            foreach (var field in part.PartDefinition.Fields)
            {
                var alias = _contentOptions.ShouldCollapse(part) ?
                    GraphQLContentOptions.GetFieldName(part, part.Name, field.Name) :
                    $"{field.PartDefinition.Name.ToFieldName()}.{field.Name.ToCamelCase()}";

                foreach (var fieldProvider in _contentFieldProviders)
                {
                    if (!fieldProvider.HasFieldIndex(field))
                    {
                        continue;
                    }

                    var fieldIndex = fieldProvider.GetFieldIndex(field);

                    if (fieldIndex is null)
                    {
                        continue;
                    }

                    tenantAliases.Add(new IndexAlias
                    {
                        Alias = alias,
                        Index = fieldIndex.Index,
                        IndexType = fieldIndex.IndexType
                    });

                    break;
                }
            }
        }

        return tenantAliases;
    }

    private void ClearAliases()
    {
        _aliases.GetOrAdd(_shellSettings.Name, _ => []).Clear();
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
