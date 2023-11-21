using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Data.Documents;

namespace OrchardCore.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string CacheKey = nameof(ContentDefinitionManager);

        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly IMemoryCache _memoryCache;

        private readonly ConcurrentDictionary<string, ContentTypeDefinition> _cachedTypeDefinitions;
        private readonly ConcurrentDictionary<string, ContentPartDefinition> _cachedPartDefinitions;

        private readonly Dictionary<string, ContentTypeDefinition> _scopedTypeDefinitions = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ContentPartDefinition> _scopedPartDefinitions = new(StringComparer.OrdinalIgnoreCase);

        public ContentDefinitionManager(
            IContentDefinitionStore contentDefinitionStore,
            IMemoryCache memoryCache)
        {
            _contentDefinitionStore = contentDefinitionStore;
            _memoryCache = memoryCache;

            _cachedTypeDefinitions = _memoryCache.GetOrCreate("TypeDefinitions", entry => new ConcurrentDictionary<string, ContentTypeDefinition>(StringComparer.OrdinalIgnoreCase));
            _cachedPartDefinitions = _memoryCache.GetOrCreate("PartDefinitions", entry => new ConcurrentDictionary<string, ContentPartDefinition>(StringComparer.OrdinalIgnoreCase));
        }

        public async Task<string> GetIdentifierAsync()
            => (await _contentDefinitionStore.GetContentDefinitionAsync()).Identifier;

        public async Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            if (!_scopedTypeDefinitions.TryGetValue(name, out var typeDefinition))
            {
                var record = await _contentDefinitionStore.LoadContentDefinitionAsync();

                var contentTypeDefinitionRecord = record
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, name));

                _scopedTypeDefinitions[name] = typeDefinition = Build(contentTypeDefinitionRecord, record.ContentPartDefinitionRecords);
            };

            return typeDefinition;
        }

        public async Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            var document = await _contentDefinitionStore.GetContentDefinitionAsync();

            CheckDocumentIdentifier(document);

            var records = document.ContentPartDefinitionRecords;

            return _cachedTypeDefinitions.GetOrAdd(name, n =>
            {
                var contentTypeDefinitionRecord = document
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, name));

                return Build(contentTypeDefinitionRecord, records);
            });
        }

        public async Task<ContentPartDefinition> LoadPartDefinitionAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            if (!_scopedPartDefinitions.TryGetValue(name, out var partDefinition))
            {
                var records = (await _contentDefinitionStore.LoadContentDefinitionAsync()).ContentPartDefinitionRecords;

                _scopedPartDefinitions[name] = partDefinition = Build(records
                    .FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, name)));
            };

            return partDefinition;
        }

        public async Task<ContentPartDefinition> GetPartDefinitionAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            var document = await _contentDefinitionStore.GetContentDefinitionAsync();

            CheckDocumentIdentifier(document);

            return _cachedPartDefinitions.GetOrAdd(name,
                n => Build(document.ContentPartDefinitionRecords.FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, name)))
            );
        }

        public async Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync()
        {
            var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

            return await GetAsync(document.ContentTypeDefinitionRecords, LoadTypeDefinitionAsync);
        }

        public async Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync()
        {
            var document = await _contentDefinitionStore.GetContentDefinitionAsync();

            return await GetAsync(document.ContentTypeDefinitionRecords, GetTypeDefinitionAsync);
        }

        public async Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync()
        {
            var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

            return await GetAsync(document.ContentPartDefinitionRecords, LoadPartDefinitionAsync);
        }

        public async Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync()
        {
            var document = await _contentDefinitionStore.GetContentDefinitionAsync();

            return await GetAsync(document.ContentPartDefinitionRecords, GetPartDefinitionAsync);
        }

        public async Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition)
        {
            var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

            Apply(contentTypeDefinition, Acquire(document, contentTypeDefinition));

            await UpdateContentDefinitionRecordAsync();
        }

        public async Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition)
        {
            var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

            Apply(contentPartDefinition, Acquire(document, contentPartDefinition));

            await UpdateContentDefinitionRecordAsync();
        }

        public async Task DeleteTypeDefinitionAsync(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            var document = await _contentDefinitionStore.LoadContentDefinitionAsync();

            var record = document.ContentTypeDefinitionRecords.FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, name));

            // Deletes the content type record associated.
            if (record != null)
            {
                document.ContentTypeDefinitionRecords.Remove(record);
                await UpdateContentDefinitionRecordAsync();
            }
        }

        public async Task DeletePartDefinitionAsync(string name)
        {
            // Remove parts from current types.
            var document = await LoadTypeDefinitionsAsync();
            var typesWithPart = document.Where(typeDefinition => typeDefinition.Parts.Any(part => EqualsOrdinalIgnoreCase(part.PartDefinition.Name, name)));

            foreach (var typeDefinition in typesWithPart)
            {
                await this.AlterTypeDefinitionAsync(typeDefinition.Name, builder => Task.FromResult(builder.RemovePart(name)));
            }

            // Delete part.
            var record = (await _contentDefinitionStore.LoadContentDefinitionAsync()).ContentPartDefinitionRecords.FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, name));

            if (record != null)
            {
                (await _contentDefinitionStore.LoadContentDefinitionAsync()).ContentPartDefinitionRecords.Remove(record);

                await UpdateContentDefinitionRecordAsync();
            }
        }

        private static ContentTypeDefinitionRecord Acquire(ContentDefinitionRecord document, ContentTypeDefinition contentTypeDefinition)
        {
            var result = document.ContentTypeDefinitionRecords.FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, contentTypeDefinition.Name));
            if (result is null)
            {
                result = new ContentTypeDefinitionRecord
                {
                    Name = contentTypeDefinition.Name,
                    DisplayName = contentTypeDefinition.DisplayName
                };
                document.ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private static ContentPartDefinitionRecord Acquire(ContentDefinitionRecord document, ContentPartDefinition contentPartDefinition)
        {
            var result = document.ContentPartDefinitionRecords.FirstOrDefault(x => EqualsOrdinalIgnoreCase(x.Name, contentPartDefinition.Name));
            if (result == null)
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
                .Where(typePartDefinitionRecord => !model.Parts.Any(part => EqualsOrdinalIgnoreCase(typePartDefinitionRecord.Name, part.Name)))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartRecord = record.ContentTypePartDefinitionRecords.FirstOrDefault(r => EqualsOrdinalIgnoreCase(r.Name, part.Name));
                if (typePartRecord == null)
                {
                    typePartRecord = new ContentTypePartDefinitionRecord
                    {
                        PartName = part.PartDefinition.Name,
                        Name = part.Name,
                        Settings = part.Settings
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
                .Where(partFieldDefinitionRecord => !model.Fields.Any(partField => EqualsOrdinalIgnoreCase(partFieldDefinitionRecord.Name, partField.Name)))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentPartFieldDefinitionRecords.Remove(remove);
            }

            foreach (var field in model.Fields)
            {
                var fieldName = field.Name;
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.FirstOrDefault(r => EqualsOrdinalIgnoreCase(r.Name, fieldName));
                if (partFieldRecord is null)
                {
                    if (field.FieldDefinition is null)
                    {
                        throw new InvalidOperationException($"The '{field.Name}' field in '{model.Name}' part was defined without a specified type. Please review the migration and explicitly specify the field type.");
                    }

                    partFieldRecord = new ContentPartFieldDefinitionRecord
                    {
                        FieldName = field.FieldDefinition.Name,
                        Name = field.Name
                    };
                    record.ContentPartFieldDefinitionRecords.Add(partFieldRecord);
                }

                Apply(field, partFieldRecord);
            }
        }

        private static void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionRecord record)
            => record.Settings = model.Settings;

        private static ContentTypeDefinition Build(ContentTypeDefinitionRecord source, IList<ContentPartDefinitionRecord> partDefinitionRecords)
        {
            if (source is null)
            {
                return null;
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(tp => Build(tp, partDefinitionRecords.FirstOrDefault(p => EqualsOrdinalIgnoreCase(p.Name, tp.PartName)))),
                source.Settings);

            return contentTypeDefinition;
        }

        private static ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source, ContentPartDefinitionRecord partDefinitionRecord)
        {
            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionRecord) ?? new ContentPartDefinition(source.PartName, [], []),
                source.Settings);
        }

        private static ContentPartDefinition Build(ContentPartDefinitionRecord source)
        {
            return source == null ? null : new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                source.Settings);
        }

        private static ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source)
        {
            return source == null ? null : new ContentPartFieldDefinition(
                Build(new ContentFieldDefinitionRecord
                {
                    Name = source.FieldName
                }),
                source.Name,
                source.Settings
            );
        }

        private static ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
            => source == null ? null : new ContentFieldDefinition(source.Name);

        private async Task UpdateContentDefinitionRecordAsync()
        {
            var contentDefinitionRecord = await _contentDefinitionStore.LoadContentDefinitionAsync();

            await _contentDefinitionStore.SaveContentDefinitionAsync(contentDefinitionRecord);

            // If multiple updates in the same scope, types and parts may need to be rebuilt.
            _scopedTypeDefinitions.Clear();
            _scopedPartDefinitions.Clear();
        }

        /// <summary>
        /// Checks the document identifier and then clears the cached built definitions if it has changed.
        /// </summary>
        private void CheckDocumentIdentifier(ContentDefinitionRecord document)
        {
            if (!_memoryCache.TryGetValue<Document>(CacheKey, out var cacheEntry) || cacheEntry.Identifier != document.Identifier)
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

        private static async Task<IEnumerable<T>> GetAsync<T, TRecord>(IEnumerable<TRecord> records, Func<string, Task<T>> getter)
            where TRecord : INamedContentDefinitionRecord
        {
            var results = new List<T>();

            foreach (var record in records)
            {
                results.Add(await getter(record.Name));
            }

            return results;
        }

        private static bool EqualsOrdinalIgnoreCase(string first, string second)
            => string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
    }
}
