using GraphQL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Events;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class DynamicContentFieldsIndexAliasProvider : ContentDefinitionHandlerBase, IIndexAliasProvider
{
    private static readonly string _cacheKey = nameof(DynamicContentFieldsIndexAliasProvider);

    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly GraphQLContentOptions _contentOptions;

    private IContentDefinitionManager _contentDefinitionManager;

    public DynamicContentFieldsIndexAliasProvider(
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        IOptions<GraphQLContentOptions> contentOptionsAccessor,
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache)
    {
        _contentFieldProviders = contentFieldProviders;
        _serviceProvider = serviceProvider;
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

        // Resolve the definition manager lazily to avoid circular dependency.
        _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

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

    public override void ContentFieldAttached(ContentFieldAttachedContext context)
        => InvalidateInternal();

    public override void ContentFieldDetached(ContentFieldDetachedContext context)
        => InvalidateInternal();

    public override void ContentPartAttached(ContentPartAttachedContext context)
        => InvalidateInternal();

    public override void ContentPartCreated(ContentPartCreatedContext context)
        => InvalidateInternal();

    public override void ContentPartDetached(ContentPartDetachedContext context)
        => InvalidateInternal();

    public override void ContentPartImported(ContentPartImportedContext context)
        => InvalidateInternal();

    public override void ContentPartRemoved(ContentPartRemovedContext context)
        => InvalidateInternal();

    public override void ContentTypeCreated(ContentTypeCreatedContext context)
        => InvalidateInternal();

    public override void ContentTypeImported(ContentTypeImportedContext context)
        => InvalidateInternal();

    public override void ContentTypeRemoved(ContentTypeRemovedContext context)
        => InvalidateInternal();

    public override void ContentTypeUpdated(ContentTypeUpdatedContext context)
        => InvalidateInternal();

    public override void ContentPartUpdated(ContentPartUpdatedContext context)
        => InvalidateInternal();

    public override void ContentTypePartUpdated(ContentTypePartUpdatedContext context)
        => InvalidateInternal();

    public override void ContentFieldUpdated(ContentFieldUpdatedContext context)
        => InvalidateInternal();

    public override void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context)
        => InvalidateInternal();
}
