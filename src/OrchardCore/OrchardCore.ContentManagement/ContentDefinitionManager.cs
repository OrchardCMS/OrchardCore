using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement;

public class ContentDefinitionManager : IContentDefinitionManager
{
    private const string CacheKey = nameof(ContentDefinitionManager);

    private readonly IContentDefinitionStore _contentDefinitionStore;
    private readonly IEnumerable<IContentDefinitionHandler> _handlers;
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;

    private readonly ConcurrentDictionary<string, ContentTypeDefinition> _cachedTypeDefinitions;
    private readonly ConcurrentDictionary<string, ContentPartDefinition> _cachedPartDefinitions;

    private readonly Dictionary<string, ContentTypeDefinition> _scopedTypeDefinitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ContentPartDefinition> _scopedPartDefinitions = new(StringComparer.OrdinalIgnoreCase);

    public ContentDefinitionManager(
        IContentDefinitionStore contentDefinitionStore,
        IEnumerable<IContentDefinitionHandler> handlers,
        ILogger<ContentDefinitionManager> logger,
        IMemoryCache memoryCache)
    {
        _contentDefinitionStore = contentDefinitionStore;
        _handlers = handlers;
        _logger = logger;
        _memoryCache = memoryCache;

        _cachedTypeDefinitions = _memoryCache.GetOrCreate("TypeDefinitions", entry => new ConcurrentDictionary<string, ContentTypeDefinition>(StringComparer.OrdinalIgnoreCase));
        _cachedPartDefinitions = _memoryCache.GetOrCreate("PartDefinitions", entry => new ConcurrentDictionary<string, ContentPartDefinition>(StringComparer.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync()
    {
        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        return document.ContentTypeDefinitionRecords.Select(type => LoadTypeDefinition(document, type.Name)).ToList();
    }

    public async Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync()
    {
        var document = await _contentDefinitionStore.GetContentDefinitionAsync();

        CheckDocumentIdentifier(document);

        return document.ContentTypeDefinitionRecords.Select(type => GetTypeDefinition(document, type.Name)).ToList();
    }

    public async Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync()
    {
        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        return document.ContentPartDefinitionRecords.Select(part => LoadPartDefinition(document, part.Name)).ToList();
    }

    public async Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync()
    {
        var document = await _contentDefinitionStore.GetContentDefinitionAsync();

        CheckDocumentIdentifier(document);

        return document.ContentPartDefinitionRecords.Select(part => GetPartDefinition(document, part.Name)).ToList();
    }

    public async Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (_scopedTypeDefinitions.TryGetValue(name, out var typeDefinition))
        {
            return typeDefinition;
        }

        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        return LoadTypeDefinition(document, name);
    }

    public async Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _contentDefinitionStore.GetContentDefinitionAsync();

        CheckDocumentIdentifier(document);

        return GetTypeDefinition(document, name);
    }

    public async Task<ContentPartDefinition> LoadPartDefinitionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (_scopedPartDefinitions.TryGetValue(name, out var partDefinition))
        {
            return partDefinition;
        }

        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        return LoadPartDefinition(document, name);
    }

    public async Task<ContentPartDefinition> GetPartDefinitionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _contentDefinitionStore.GetContentDefinitionAsync();

        CheckDocumentIdentifier(document);

        return GetPartDefinition(document, name);
    }

    public async Task DeleteTypeDefinitionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        var record = document.ContentTypeDefinitionRecords.FirstOrDefault(record => record.Name.EqualsOrdinalIgnoreCase(name));

        // Deletes the content type record associated.
        if (record is not null)
        {
            document.ContentTypeDefinitionRecords.Remove(record);
            await UpdateContentDefinitionRecordAsync(document);
        }
    }

    public async Task DeletePartDefinitionAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        // Remove parts from current types.
        var typeDefinitions = document.ContentTypeDefinitionRecords
            .Select(type => LoadTypeDefinition(document, type.Name)).ToList();

        var typesWithPart = typeDefinitions
            .Where(typeDefinition => typeDefinition.Parts
                .Any(part => part.PartDefinition.Name.EqualsOrdinalIgnoreCase(name)));

        foreach (var typeDefinition in typesWithPart)
        {
            await this.AlterTypeDefinitionAsync(typeDefinition.Name, builder => Task.FromResult(builder.RemovePart(name)));
        }

        // Delete part.
        var record = document.ContentPartDefinitionRecords.FirstOrDefault(part => part.Name.EqualsOrdinalIgnoreCase(name));
        if (record is not null)
        {
            document.ContentPartDefinitionRecords.Remove(record);
            await UpdateContentDefinitionRecordAsync(document);
        }
    }

    public async Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition)
    {
        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        Apply(contentTypeDefinition, Acquire(document, contentTypeDefinition));

        await UpdateContentDefinitionRecordAsync(document);
    }

    public async Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition)
    {
        var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

        Apply(contentPartDefinition, Acquire(document, contentPartDefinition));

        await UpdateContentDefinitionRecordAsync(document);
    }

    public async Task<string> GetIdentifierAsync()
        => (await _contentDefinitionStore.GetContentDefinitionAsync()).Identifier;

    private ContentTypeDefinition LoadTypeDefinition(ContentDefinitionRecord document, string name) =>
        _scopedTypeDefinitions.TryGetValue(name, out var typeDefinition)
        ? typeDefinition
        : _scopedTypeDefinitions[name] = Build(
            name,
            document.ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name.EqualsOrdinalIgnoreCase(name)),
            document.ContentPartDefinitionRecords);

    private ContentTypeDefinition GetTypeDefinition(ContentDefinitionRecord document, string name) =>
        _cachedTypeDefinitions.GetOrAdd(name, name => Build(
            name,
            document.ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name.EqualsOrdinalIgnoreCase(name)),
            document.ContentPartDefinitionRecords));

    private ContentPartDefinition LoadPartDefinition(ContentDefinitionRecord document, string name) =>
        !_scopedPartDefinitions.TryGetValue(name, out var partDefinition)
        ? _scopedPartDefinitions[name] = Build(
            name,
            document.ContentPartDefinitionRecords.FirstOrDefault(part => part.Name.EqualsOrdinalIgnoreCase(name)))
        : partDefinition;

    private ContentPartDefinition GetPartDefinition(ContentDefinitionRecord document, string name) =>
        _cachedPartDefinitions.GetOrAdd(name, name => Build(
            name,
            document.ContentPartDefinitionRecords.FirstOrDefault(record => record.Name.EqualsOrdinalIgnoreCase(name))));

    private static ContentTypeDefinitionRecord Acquire(
        ContentDefinitionRecord document,
        ContentTypeDefinition contentTypeDefinition)
    {
        var result = document.ContentTypeDefinitionRecords
            .FirstOrDefault(type => type.Name.EqualsOrdinalIgnoreCase(contentTypeDefinition.Name));

        if (result is null)
        {
            result = new ContentTypeDefinitionRecord
            {
                Name = contentTypeDefinition.Name,
                DisplayName = contentTypeDefinition.DisplayName,
            };

            document.ContentTypeDefinitionRecords.Add(result);
        }

        return result;
    }

    private static ContentPartDefinitionRecord Acquire(
        ContentDefinitionRecord document,
        ContentPartDefinition contentPartDefinition)
    {
        var result = document.ContentPartDefinitionRecords
            .FirstOrDefault(part => part.Name.EqualsOrdinalIgnoreCase(contentPartDefinition.Name));

        if (result is null)
        {
            result = new ContentPartDefinitionRecord
            {
                Name = contentPartDefinition.Name,
            };

            document.ContentPartDefinitionRecords.Add(result);
        }

        return result;
    }

    private static void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record)
    {
        record.DisplayName = model.DisplayName;
        record.Settings = model.Settings;

        var toRemove = record.ContentTypePartDefinitionRecords
            .Where(typePartDefinitionRecord => !model.Parts
                .Any(typePart => typePart.Name.EqualsOrdinalIgnoreCase(typePartDefinitionRecord.Name)))
            .ToArray();

        foreach (var remove in toRemove)
        {
            record.ContentTypePartDefinitionRecords.Remove(remove);
        }

        foreach (var part in model.Parts)
        {
            var typePartRecord = record.ContentTypePartDefinitionRecords
                .FirstOrDefault(typePart => typePart.Name.EqualsOrdinalIgnoreCase(part.Name));

            if (typePartRecord is null)
            {
                typePartRecord = new ContentTypePartDefinitionRecord
                {
                    PartName = part.PartDefinition.Name,
                    Name = part.Name,
                    Settings = part.Settings,
                };

                record.ContentTypePartDefinitionRecords.Add(typePartRecord);
            }

            Apply(part, typePartRecord);
        }
    }

    private static void Apply(ContentTypePartDefinition model, ContentTypePartDefinitionRecord record)
        => record.Settings = model.Settings;

    private static void Apply(ContentPartDefinition model, ContentPartDefinitionRecord record)
    {
        record.Settings = model.Settings;

        var toRemove = record.ContentPartFieldDefinitionRecords
            .Where(partFieldDefinitionRecord => !model.Fields
                .Any(partField => partField.Name.EqualsOrdinalIgnoreCase(partFieldDefinitionRecord.Name)))
            .ToArray();

        foreach (var remove in toRemove)
        {
            record.ContentPartFieldDefinitionRecords.Remove(remove);
        }

        foreach (var field in model.Fields)
        {
            var partFieldRecord = record.ContentPartFieldDefinitionRecords
                .FirstOrDefault(partField => partField.Name.EqualsOrdinalIgnoreCase(field.Name));

            if (partFieldRecord is null)
            {
                if (field.FieldDefinition is null)
                {
                    throw new InvalidOperationException(
                        $"The '{field.Name}' field in '{model.Name}' part was defined without a specified type." +
                        " Please review the migration and explicitly specify the field type.");
                }

                partFieldRecord = new ContentPartFieldDefinitionRecord
                {
                    FieldName = field.FieldDefinition.Name,
                    Name = field.Name,
                };

                record.ContentPartFieldDefinitionRecords.Add(partFieldRecord);
            }

            Apply(field, partFieldRecord);
        }
    }

    private static void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionRecord record)
        => record.Settings = model.Settings;

    private ContentTypeDefinition Build(
        string name,
        ContentTypeDefinitionRecord source,
        IList<ContentPartDefinitionRecord> partDefinitionRecords)
    {
        var context = new ContentTypeBuildingContext(name, source);

        _handlers.Invoke((handler, ctx) => handler.ContentTypeBuilding(ctx), context, _logger);

        if (context.Record is null)
        {
            return null;
        }

        return new ContentTypeDefinition(
            context.Record.Name,
            context.Record.DisplayName,
            context.Record.ContentTypePartDefinitionRecords.Select(typePart => Build(context.Record.Name,
                typePart,
                partDefinitionRecords.FirstOrDefault(part => part.Name.EqualsOrdinalIgnoreCase(typePart.PartName)))),
            context.Record.Settings);
    }

    private ContentTypePartDefinition Build(
        string name,
        ContentTypePartDefinitionRecord source,
        ContentPartDefinitionRecord partDefinitionRecord)
    {
        var context = new ContentTypePartBuildingContext(name, source);

        _handlers.Invoke((handler, ctx) => handler.ContentTypePartBuilding(ctx), context, _logger);

        if (context.Record is null)
        {
            return null;
        }

        return new ContentTypePartDefinition(
            context.Record.Name,
            Build(context.Record.Name, partDefinitionRecord) ?? new ContentPartDefinition(context.Record.PartName, [], []),
            context.Record.Settings);
    }

    private ContentPartDefinition Build(string name, ContentPartDefinitionRecord source)
    {
        var context = new ContentPartBuildingContext(name, source);

        _handlers.Invoke((handler, ctx) => handler.ContentPartBuilding(ctx), context, _logger);

        if (context.Record is null)
        {
            return null;
        }

        return new ContentPartDefinition(
            context.Record.Name,
            context.Record.ContentPartFieldDefinitionRecords.Select(x => Build(context.Record.Name, x)),
            context.Record.Settings);
    }

    private ContentPartFieldDefinition Build(string name, ContentPartFieldDefinitionRecord source)
    {
        var context = new ContentPartFieldBuildingContext(name, source);

        _handlers.Invoke((handler, ctx) => handler.ContentPartFieldBuilding(ctx), context, _logger);

        if (context.Record is null)
        {
            return null;
        }

        return new ContentPartFieldDefinition(
            Build(new ContentFieldDefinitionRecord
            {
                Name = context.Record.FieldName,
            }),
            context.Record.Name,
            context.Record.Settings);
    }

    private static ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
    {
        if (source is null)
        {
            return null;
        }

        return new ContentFieldDefinition(source.Name);
    }

    private async Task UpdateContentDefinitionRecordAsync(ContentDefinitionRecord document)
    {
        await _contentDefinitionStore.SaveContentDefinitionAsync(document);

        // If multiple updates in the same scope, types and parts may need to be rebuilt.
        _scopedTypeDefinitions.Clear();
        _scopedPartDefinitions.Clear();
    }

    /// <summary>
    /// Checks the document identifier and then clears the cached built definitions if it has changed.
    /// </summary>
    private void CheckDocumentIdentifier(ContentDefinitionRecord document)
    {
        if (!_memoryCache.TryGetValue<Document>(CacheKey, out var cacheEntry) ||
            cacheEntry.Identifier != document.Identifier)
        {
            cacheEntry = new Document()
            {
                Identifier = document.Identifier,
            };

            _cachedTypeDefinitions.Clear();
            _cachedPartDefinitions.Clear();

            _memoryCache.Set(CacheKey, cacheEntry);
        }
    }
}
