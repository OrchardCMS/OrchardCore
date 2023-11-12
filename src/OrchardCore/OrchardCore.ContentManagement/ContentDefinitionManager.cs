using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
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

        public ContentTypeDefinition LoadTypeDefinition(string name)
            => LoadTypeDefinitionAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<ContentTypeDefinition> LoadTypeDefinitionAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(name));
            }

            if (!_scopedTypeDefinitions.TryGetValue(name, out var typeDefinition))
            {
                var record = await LoadContentDefinitionRecordAsync();

                var contentTypeDefinitionRecord = record
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                _scopedTypeDefinitions[name] = typeDefinition = Build(contentTypeDefinitionRecord, record.ContentPartDefinitionRecords);
            };

            return typeDefinition;
        }

        public ContentTypeDefinition GetTypeDefinition(string name)
            => GetTypeDefinitionAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(name));
            }

            var document = await GetContentDefinitionRecordAsync();
            CheckDocumentIdentifier(document);

            var records = document.ContentPartDefinitionRecords;

            return _cachedTypeDefinitions.GetOrAdd(name, n =>
            {
                var contentTypeDefinitionRecord = document
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

                return Build(contentTypeDefinitionRecord, records);
            });
        }

        public ContentPartDefinition LoadPartDefinition(string name)
            => LoadPartDefinitionAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<ContentPartDefinition> LoadPartDefinitionAsync(string name)
        {
            if (!_scopedPartDefinitions.TryGetValue(name, out var partDefinition))
            {
                var record = await LoadContentDefinitionRecordAsync();

                _scopedPartDefinitions[name] = partDefinition = Build(record
                    .ContentPartDefinitionRecords
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
            };

            return partDefinition;
        }

        public ContentPartDefinition GetPartDefinition(string name)
            => GetPartDefinitionAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<ContentPartDefinition> GetPartDefinitionAsync(string name)
        {
            var document = await GetContentDefinitionRecordAsync();
            CheckDocumentIdentifier(document);

            return _cachedPartDefinitions.GetOrAdd(name, n =>
            {
                return Build(document
                    .ContentPartDefinitionRecords
                    .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
            });
        }

        public IEnumerable<ContentTypeDefinition> LoadTypeDefinitions()
            => LoadTypeDefinitionsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<IEnumerable<ContentTypeDefinition>> LoadTypeDefinitionsAsync()
        {
            var document = await LoadContentDefinitionRecordAsync();

            var records = new List<ContentTypeDefinition>();

            foreach (var record in document.ContentTypeDefinitionRecords)
            {
                records.Add(await LoadTypeDefinitionAsync(record.Name));
            }

            return records;
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
            => ListTypeDefinitionsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync()
        {
            var document = await GetContentDefinitionRecordAsync();

            var records = new List<ContentTypeDefinition>();

            foreach (var record in document.ContentTypeDefinitionRecords)
            {
                records.Add(await GetTypeDefinitionAsync(record.Name));
            }

            return records;
        }

        public IEnumerable<ContentPartDefinition> LoadPartDefinitions()
            => LoadPartDefinitionsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<IEnumerable<ContentPartDefinition>> LoadPartDefinitionsAsync()
        {
            var document = await LoadContentDefinitionRecordAsync();

            var records = new List<ContentPartDefinition>();

            foreach (var record in document.ContentPartDefinitionRecords)
            {
                records.Add(await LoadPartDefinitionAsync(record.Name));
            }

            return records;
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions()
            => ListPartDefinitionsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync()
        {
            var document = await GetContentDefinitionRecordAsync();

            var records = new List<ContentPartDefinition>();

            foreach (var record in document.ContentPartDefinitionRecords)
            {
                records.Add(await GetPartDefinitionAsync(record.Name));
            }

            return records;
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
            => StoreTypeDefinitionAsync(contentTypeDefinition).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition)
        {
            Apply(contentTypeDefinition, await AcquireAsync(contentTypeDefinition));

            await UpdateContentDefinitionRecordAsync();
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition)
            => StorePartDefinitionAsync(contentPartDefinition).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition)
        {
            Apply(contentPartDefinition, await AcquireAsync(contentPartDefinition));

            await UpdateContentDefinitionRecordAsync();
        }

        public void DeleteTypeDefinition(string name)
            => DeleteTypeDefinitionAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task DeleteTypeDefinitionAsync(string name)
        {
            var document = await LoadContentDefinitionRecordAsync();

            var record = document.ContentTypeDefinitionRecords.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            // deletes the content type record associated
            if (record != null)
            {
                document.ContentTypeDefinitionRecords.Remove(record);
                await UpdateContentDefinitionRecordAsync();
            }
        }

        public void DeletePartDefinition(string name)
            => DeletePartDefinitionAsync(name).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task DeletePartDefinitionAsync(string name)
        {
            // remove parts from current types
            var document = await LoadTypeDefinitionsAsync();
            var typesWithPart = document.Where(typeDefinition => typeDefinition.Parts.Any(part => string.Equals(part.PartDefinition.Name, name, StringComparison.OrdinalIgnoreCase)));

            foreach (var typeDefinition in typesWithPart)
            {
                await this.AlterTypeDefinitionAsync(typeDefinition.Name, builder => Task.FromResult(builder.RemovePart(name)));
            }

            // delete part
            var record = (await LoadContentDefinitionRecordAsync()).ContentPartDefinitionRecords.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (record != null)
            {
                (await LoadContentDefinitionRecordAsync()).ContentPartDefinitionRecords.Remove(record);
                await UpdateContentDefinitionRecordAsync();
            }
        }

        private async Task<ContentTypeDefinitionRecord> AcquireAsync(ContentTypeDefinition contentTypeDefinition)
        {
            var document = await LoadContentDefinitionRecordAsync();

            var result = document.ContentTypeDefinitionRecords.FirstOrDefault(x => string.Equals(x.Name, contentTypeDefinition.Name, StringComparison.OrdinalIgnoreCase));
            if (result == null)
            {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                (await LoadContentDefinitionRecordAsync()).ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private async Task<ContentPartDefinitionRecord> AcquireAsync(ContentPartDefinition contentPartDefinition)
        {
            var document = await LoadContentDefinitionRecordAsync();

            var result = document.ContentPartDefinitionRecords.FirstOrDefault(x => string.Equals(x.Name, contentPartDefinition.Name, StringComparison.OrdinalIgnoreCase));
            if (result == null)
            {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name, };
                (await LoadContentDefinitionRecordAsync()).ContentPartDefinitionRecords.Add(result);
            }
            return result;
        }

        private static void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record)
        {
            record.DisplayName = model.DisplayName;
            record.Settings = model.Settings;

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(typePartDefinitionRecord => !model.Parts.Any(part => string.Equals(typePartDefinitionRecord.Name, part.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartRecord = record.ContentTypePartDefinitionRecords.FirstOrDefault(r => string.Equals(r.Name, part.Name, StringComparison.OrdinalIgnoreCase));
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
        {
            record.Settings = model.Settings;
        }

        private static void Apply(ContentPartDefinition model, ContentPartDefinitionRecord record)
        {
            record.Settings = model.Settings;

            var toRemove = record.ContentPartFieldDefinitionRecords
                .Where(partFieldDefinitionRecord => !model.Fields.Any(partField => string.Equals(partFieldDefinitionRecord.Name, partField.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentPartFieldDefinitionRecords.Remove(remove);
            }

            foreach (var field in model.Fields)
            {
                var fieldName = field.Name;
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.FirstOrDefault(r => string.Equals(r.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                if (partFieldRecord == null)
                {
                    if (field.FieldDefinition == null)
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
        {
            record.Settings = model.Settings;
        }

        private ContentTypeDefinition Build(ContentTypeDefinitionRecord source, IList<ContentPartDefinitionRecord> partDefinitionRecords)
        {
            if (source == null)
            {
                return null;
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(tp => Build(tp, partDefinitionRecords.FirstOrDefault(p => string.Equals(p.Name, tp.PartName, StringComparison.OrdinalIgnoreCase)))),
                source.Settings);

            return contentTypeDefinition;
        }

        private ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source, ContentPartDefinitionRecord partDefinitionRecord)
        {
            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionRecord) ?? new ContentPartDefinition(source.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), new JObject()),
                source.Settings);
        }

        private ContentPartDefinition Build(ContentPartDefinitionRecord source)
        {
            return source == null ? null : new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                source.Settings);
        }

        private ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source)
        {
            return source == null ? null : new ContentPartFieldDefinition(
                Build(new ContentFieldDefinitionRecord { Name = source.FieldName }),
                source.Name,
                source.Settings
            );
        }

        private static ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
            => source == null ? null : new ContentFieldDefinition(source.Name);

        private Task<ContentDefinitionRecord> LoadContentDefinitionRecordAsync()
            => _contentDefinitionStore.LoadContentDefinitionAsync();

        private Task<ContentDefinitionRecord> GetContentDefinitionRecordAsync()
            => _contentDefinitionStore.GetContentDefinitionAsync();

        private async Task UpdateContentDefinitionRecordAsync()
        {
            var contentDefinitionRecord = await LoadContentDefinitionRecordAsync();

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
    }
}
