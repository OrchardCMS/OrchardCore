using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string CacheKey = nameof(ContentDefinitionManager);

        private readonly ISignal _signal;
        private ContentDefinitionCache _cache;
        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly ConcurrentDictionary<string, ContentTypeDefinition> _typeDefinitions;
        private readonly ConcurrentDictionary<string, ContentPartDefinition> _partDefinitions;

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        public ContentDefinitionManager(
            ISignal signal,
            IContentDefinitionStore contentDefinitionStore)
        {
            _signal = signal;
            _contentDefinitionStore = contentDefinitionStore;

            _typeDefinitions = new ConcurrentDictionary<string, ContentTypeDefinition>();
            _partDefinitions = new ConcurrentDictionary<string, ContentPartDefinition>();
        }

        private ContentDefinitionCache ScopedCache => ShellScope.Services.GetRequiredService<ContentDefinitionCache>();

        public async Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name)
        {
            if (!_typeDefinitions.TryGetValue(name, out var typeDefinition))
            {
                typeDefinition = await BuildAsync((await GetContentDefinitionRecordAsync())
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(type => type.Name == name));

                // Don't cache a value based on a stale definition.
                if (!ScopedCache.ChangeToken.HasChanged)
                {
                    _typeDefinitions[name] = typeDefinition;
                }
            }

            return typeDefinition;
        }

        public async Task<ContentPartDefinition> GetPartDefinitionAsync(string name)
        {
            if (!_partDefinitions.TryGetValue(name, out var partDefinition))
            {
                partDefinition = Build((await GetContentDefinitionRecordAsync())
                    .ContentPartDefinitionRecords
                    .FirstOrDefault(part => part.Name == name));

                // Don't cache a value based on a stale definition.
                if (!ScopedCache.ChangeToken.HasChanged)
                {
                    _partDefinitions[name] = partDefinition;
                }
            }

            return partDefinition;
        }

        public async Task<IEnumerable<ContentTypeDefinition>> ListTypeDefinitionsAsync()
        {
            var typeDefinitions = new List<ContentTypeDefinition>();

            foreach (var record in (await GetContentDefinitionRecordAsync()).ContentTypeDefinitionRecords)
            {
                typeDefinitions.Add(await GetTypeDefinitionAsync(record.Name));
            }

            return typeDefinitions;
        }

        public async Task<IEnumerable<ContentPartDefinition>> ListPartDefinitionsAsync()
        {
            var partDefinitions = new List<ContentPartDefinition>();

            foreach (var record in (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords)
            {
                partDefinitions.Add(await GetPartDefinitionAsync(record.Name));
            }

            return partDefinitions;
        }

        public async Task StoreTypeDefinitionAsync(ContentTypeDefinition contentTypeDefinition)
        {
            Apply(contentTypeDefinition, await AcquireAsync(contentTypeDefinition));
            await UpdateContentDefinitionRecordAsync();
        }

        public async Task StorePartDefinitionAsync(ContentPartDefinition contentPartDefinition)
        {
            Apply(contentPartDefinition, await AcquireAsync(contentPartDefinition));
            await UpdateContentDefinitionRecordAsync();
        }

        public async Task DeleteTypeDefinitionAsync(string name)
        {
            var record = (await GetContentDefinitionRecordAsync()).ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name == name);

            // deletes the content type record associated
            if (record != null)
            {
                var definition = await GetContentDefinitionRecordAsync();
                definition.ContentTypeDefinitionRecords = definition.ContentTypeDefinitionRecords.Remove(record);
                await UpdateContentDefinitionRecordAsync();
            }
        }

        public async Task DeletePartDefinitionAsync(string name)
        {
            // remove parts from current types
            var typesWithPart = (await ListTypeDefinitionsAsync()).Where(typeDefinition => typeDefinition.Parts.Any(part => part.PartDefinition.Name == name));

            foreach (var typeDefinition in typesWithPart)
            {
                await this.AlterTypeDefinitionAsync(typeDefinition.Name, builder => builder.RemovePart(name));
            }

            // delete part
            var record = (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords.FirstOrDefault(part => part.Name == name);

            if (record != null)
            {
                var definition = await GetContentDefinitionRecordAsync();
                definition.ContentPartDefinitionRecords = definition.ContentPartDefinitionRecords.Remove(record);
                await UpdateContentDefinitionRecordAsync();
            }
        }

        private async Task<ContentTypeDefinitionRecord> AcquireAsync(ContentTypeDefinition contentTypeDefinition)
        {
            var result = (await GetContentDefinitionRecordAsync()).ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                var definition = await GetContentDefinitionRecordAsync();
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                definition.ContentTypeDefinitionRecords = definition.ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private async Task<ContentPartDefinitionRecord> AcquireAsync(ContentPartDefinition contentPartDefinition)
        {
            var result = (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords.FirstOrDefault(part => part.Name == contentPartDefinition.Name);
            if (result == null)
            {
                var definition = await GetContentDefinitionRecordAsync();
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name, };
                definition.ContentPartDefinitionRecords = definition.ContentPartDefinitionRecords.Add(result);
            }
            return result;
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record)
        {
            record.DisplayName = model.DisplayName;
            record.Settings = model.Settings;

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(typePartDefinitionRecord => !model.Parts.Any(typePart => typePartDefinitionRecord.Name == typePart.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitionRecords = record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartRecord = record.ContentTypePartDefinitionRecords.FirstOrDefault(typePart => typePart.Name == part.Name);

                var newTypePartRecord = new ContentTypePartDefinitionRecord
                {
                    PartName = typePartRecord?.PartName ?? part.PartDefinition.Name,
                    Name = typePartRecord?.Name ?? part.Name
                };

                Apply(part, newTypePartRecord);

                if (typePartRecord == null)
                {
                    record.ContentTypePartDefinitionRecords = record.ContentTypePartDefinitionRecords.Add(newTypePartRecord);
                }
                else
                {
                    record.ContentTypePartDefinitionRecords = record.ContentTypePartDefinitionRecords.Replace(typePartRecord, newTypePartRecord);
                }
            }
        }

        private void Apply(ContentTypePartDefinition model, ContentTypePartDefinitionRecord record)
        {
            record.Settings = model.Settings;
        }

        private void Apply(ContentPartDefinition model, ContentPartDefinitionRecord record)
        {
            record.Settings = model.Settings;

            var toRemove = record.ContentPartFieldDefinitionRecords
                .Where(partFieldDefinitionRecord => !model.Fields.Any(partField => partFieldDefinitionRecord.Name == partField.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentPartFieldDefinitionRecords = record.ContentPartFieldDefinitionRecords.Remove(remove);
            }

            foreach (var field in model.Fields)
            {
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.FirstOrDefault(r => r.Name == field.Name);

                var newPartFieldRecord = new ContentPartFieldDefinitionRecord
                {
                    FieldName = partFieldRecord?.FieldName ?? field.FieldDefinition.Name,
                    Name = partFieldRecord?.Name ?? field.Name
                };

                Apply(field, newPartFieldRecord);

                if (partFieldRecord == null)
                {
                    record.ContentPartFieldDefinitionRecords = record.ContentPartFieldDefinitionRecords.Add(newPartFieldRecord);
                }
                else
                {
                    record.ContentPartFieldDefinitionRecords = record.ContentPartFieldDefinitionRecords.Replace(partFieldRecord, newPartFieldRecord);
                }
            }
        }

        private void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionRecord record)
        {
            record.Settings = model.Settings;
        }

        async Task<ContentTypeDefinition> BuildAsync(ContentTypeDefinitionRecord source)
        {
            if (source == null)
            {
                return null;
            }

            var typePartDefinitions = new List<ContentTypePartDefinition>();
            foreach (var typePartDefinition in source.ContentTypePartDefinitionRecords)
            {
                typePartDefinitions.Add(await BuildAsync(typePartDefinition));
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                typePartDefinitions,
                source.Settings);

            return contentTypeDefinition;
        }

        async Task<ContentTypePartDefinition> BuildAsync(ContentTypePartDefinitionRecord source)
        {
            var partDefinitionRecord = (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords.FirstOrDefault(part => part.Name == source.PartName);

            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionRecord) ?? new ContentPartDefinition(source.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), new Newtonsoft.Json.Linq.JObject()),
                source.Settings);
        }

        ContentPartDefinition Build(ContentPartDefinitionRecord source)
        {
            return source == null ? null : new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                source.Settings);
        }

        ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source)
        {
            return source == null ? null : new ContentPartFieldDefinition(
                Build(new ContentFieldDefinitionRecord { Name = source.FieldName }),
                source.Name,
                source.Settings
            );
        }

        ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
        {
            return source == null ? null : new ContentFieldDefinition(source.Name);
        }

        public async Task<int> GetTypesHashAsync()
        {
            return (await GetContentDefinitionRecordAsync()).Serial;
        }

        private async Task<ContentDefinitionRecord> GetContentDefinitionRecordAsync()
        {
            var scopedCache = ScopedCache;

            if (scopedCache.ContentDefinitionRecord != null)
            {
                return scopedCache.ContentDefinitionRecord;
            }

            var cache = _cache;

            if (cache?.ChangeToken.HasChanged ?? true)
            {
                var changeToken = ChangeToken;
                var record = await _contentDefinitionStore.LoadContentDefinitionAsync();

                _cache = new ContentDefinitionCache()
                {
                    ChangeToken = changeToken,
                    ContentDefinitionRecord = record.Clone()
                };

                scopedCache.ChangeToken = changeToken;
                return scopedCache.ContentDefinitionRecord = record;
            }

            scopedCache.ChangeToken = cache.ChangeToken;
            return scopedCache.ContentDefinitionRecord = cache.ContentDefinitionRecord.Clone();
        }

        private async Task UpdateContentDefinitionRecordAsync()
        {
            var scopedRecord = ScopedCache.ContentDefinitionRecord;
            scopedRecord.Serial++;

            await _contentDefinitionStore.SaveContentDefinitionAsync(scopedRecord);

            // Cache invalidation after committing the session.
            _signal.DeferredSignalToken(CacheKey);

            // In case of multiple scoped mutations, types / parts may need to be
            // rebuilt while in the same scope, so we release cached results here.
            _typeDefinitions.Clear();
            _partDefinitions.Clear();
        }
    }
}
