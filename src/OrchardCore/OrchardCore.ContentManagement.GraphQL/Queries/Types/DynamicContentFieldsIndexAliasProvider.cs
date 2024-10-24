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

    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IMemoryCache _memoryCache;
    private readonly GraphQLContentOptions _contentOptions;


    public DynamicContentFieldsIndexAliasProvider(
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        IOptions<GraphQLContentOptions> contentOptionsAccessor,
        IContentDefinitionManager contentDefinitionManager,
        IMemoryCache memoryCache)
    {
        _contentFieldProviders = contentFieldProviders;
        _contentDefinitionManager = contentDefinitionManager;
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

    private void InvalidateInternal()
        => _memoryCache.Remove(_cacheKey);

    public void ContentFieldAttached(ContentFieldAttachedContext context)
        => InvalidateInternal();

    public void ContentFieldDetached(ContentFieldDetachedContext context)
        => InvalidateInternal();

    public void ContentPartAttached(ContentPartAttachedContext context)
        => InvalidateInternal();

    public void ContentPartCreated(ContentPartCreatedContext context)
        => InvalidateInternal();

    public void ContentPartDetached(ContentPartDetachedContext context)
        => InvalidateInternal();

    public void ContentPartImported(ContentPartImportedContext context)
        => InvalidateInternal();

    public void ContentPartRemoved(ContentPartRemovedContext context)
        => InvalidateInternal();

    public void ContentTypeCreated(ContentTypeCreatedContext context)
        => InvalidateInternal();

    public void ContentTypeImported(ContentTypeImportedContext context)
        => InvalidateInternal();

    public void ContentTypeRemoved(ContentTypeRemovedContext context)
        => InvalidateInternal();

    public void ContentTypeUpdated(ContentTypeUpdatedContext context)
        => InvalidateInternal();

    public void ContentPartUpdated(ContentPartUpdatedContext context)
        => InvalidateInternal();

    public void ContentTypePartUpdated(ContentTypePartUpdatedContext context)
        => InvalidateInternal();

    public void ContentFieldUpdated(ContentFieldUpdatedContext context)
        => InvalidateInternal();

    public void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context)
        => InvalidateInternal();

    public void ContentTypeImporting(ContentTypeImportingContext context)
    {
    }

    public void ContentPartImporting(ContentPartImportingContext context)
    {
    }
}
