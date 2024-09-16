using GraphQL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Events;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class DynamicContentFieldsIndexAliasProvider : IIndexAliasProvider, IContentDefinitionEventHandler
{
    private static readonly string _cacheKey = nameof(DynamicContentFieldsIndexAliasProvider);

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
    private readonly IMemoryCache _memoryCache;
    private readonly GraphQLContentOptions _contentOptions;

    public DynamicContentFieldsIndexAliasProvider(IContentDefinitionManager contentDefinitionManager,
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        IOptions<GraphQLContentOptions> contentOptionsAccessor,
        IMemoryCache memoryCache)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentFieldProviders = contentFieldProviders;
        _memoryCache = memoryCache;
        _contentOptions = contentOptionsAccessor.Value;
    }

    public async ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return await _memoryCache.GetOrCreateAsync(_cacheKey, async _ => await GetAliasesInternalAsync());
    }

    private async ValueTask<IEnumerable<IndexAlias>> GetAliasesInternalAsync()
    {
        var aliases = new List<IndexAlias>();
        var types = await _contentDefinitionManager.ListTypeDefinitionsAsync();
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

                    aliases.Add(new IndexAlias
                    {
                        Alias = alias,
                        Index = fieldIndex.Index,
                        IndexType = fieldIndex.IndexType
                    });

                    break;
                }
            }
        }

        return aliases;
    }

    private void InvalidateInternalAsync() => _memoryCache.Remove(_cacheKey);

    public void ContentFieldAttached(ContentFieldAttachedContext context) => InvalidateInternalAsync();

    public void ContentFieldDetached(ContentFieldDetachedContext context) => InvalidateInternalAsync();

    public void ContentPartAttached(ContentPartAttachedContext context) => InvalidateInternalAsync();

    public void ContentPartCreated(ContentPartCreatedContext context) => InvalidateInternalAsync();

    public void ContentPartDetached(ContentPartDetachedContext context) => InvalidateInternalAsync();

    public void ContentPartImported(ContentPartImportedContext context) => InvalidateInternalAsync();

    public void ContentPartImporting(ContentPartImportingContext context) { }

    public void ContentPartRemoved(ContentPartRemovedContext context) => InvalidateInternalAsync();

    public void ContentTypeCreated(ContentTypeCreatedContext context) => InvalidateInternalAsync();

    public void ContentTypeImported(ContentTypeImportedContext context) => InvalidateInternalAsync();

    public void ContentTypeImporting(ContentTypeImportingContext context) { }

    public void ContentTypeRemoved(ContentTypeRemovedContext context) => InvalidateInternalAsync();

    public void ContentTypeUpdated(ContentTypeUpdatedContext context) => InvalidateInternalAsync();

    public void ContentPartUpdated(ContentPartUpdatedContext context) => InvalidateInternalAsync();

    public void ContentTypePartUpdated(ContentTypePartUpdatedContext context) => InvalidateInternalAsync();

    public void ContentFieldUpdated(ContentFieldUpdatedContext context) => InvalidateInternalAsync();

    public void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context) => InvalidateInternalAsync();
}
