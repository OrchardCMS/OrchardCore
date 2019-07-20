using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Environment.Cache;

namespace OrchardCore.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string TypeHashCacheKey = "ContentDefinitionManager:Serial";

        private ContentDefinitionRecord _contentDefinitionRecord;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly ConcurrentDictionary<string, ContentTypeDefinition> _typeDefinitions;
        private readonly ConcurrentDictionary<string, ContentPartDefinition> _partDefinitions;

        public IChangeToken ChangeToken => _signal.GetToken(TypeHashCacheKey);

        public ContentDefinitionManager(
            IMemoryCache memoryCache,
            ISignal signal,
            IContentDefinitionStore contentDefinitionStore)
        {
            _signal = signal;
            _contentDefinitionStore = contentDefinitionStore;
            _memoryCache = memoryCache;

            _typeDefinitions = _memoryCache.GetOrCreate("TypeDefinitions", entry => new ConcurrentDictionary<string, ContentTypeDefinition>());
            _partDefinitions = _memoryCache.GetOrCreate("PartDefinitions", entry => new ConcurrentDictionary<string, ContentPartDefinition>());
        }

        public async Task<ContentTypeDefinition> GetTypeDefinitionAsync(string name)
        {
            if (!_typeDefinitions.TryGetValue(name, out var typeDefinition))
            {
                typeDefinition = await BuildAsync((await GetContentDefinitionRecordAsync())
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(type => type.Name == name));

                _typeDefinitions[name] = typeDefinition;
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

                _partDefinitions[name] = partDefinition;
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
            await ApplyAsync(contentTypeDefinition, await AcquireAsync(contentTypeDefinition));
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
                (await GetContentDefinitionRecordAsync()).ContentTypeDefinitionRecords.Remove(record);
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
                (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords.Remove(record);
                await UpdateContentDefinitionRecordAsync();
            }
        }

        private async Task<ContentTypeDefinitionRecord> AcquireAsync(ContentTypeDefinition contentTypeDefinition)
        {
            var result = (await GetContentDefinitionRecordAsync()).ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                (await GetContentDefinitionRecordAsync()).ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private async Task<ContentPartDefinitionRecord> AcquireAsync(ContentPartDefinition contentPartDefinition)
        {
            var result = (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords.FirstOrDefault(part => part.Name == contentPartDefinition.Name);
            if (result == null)
            {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name, };
                (await GetContentDefinitionRecordAsync()).ContentPartDefinitionRecords.Add(result);
            }
            return result;
        }

        private Task ApplyAsync(ContentTypeDefinition model, ContentTypeDefinitionRecord record)
        {
            record.DisplayName = model.DisplayName;
            record.Settings = model.Settings;

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(typePartDefinitionRecord => !model.Parts.Any(typePart => typePartDefinitionRecord.Name == typePart.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartRecord = record.ContentTypePartDefinitionRecords.FirstOrDefault(typePart => typePart.Name == part.Name);
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

            // Persist changes
            return UpdateContentDefinitionRecordAsync();
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
                record.ContentPartFieldDefinitionRecords.Remove(remove);
            }

            foreach (var field in model.Fields)
            {
                var fieldName = field.Name;
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.FirstOrDefault(r => r.Name == fieldName);
                if (partFieldRecord == null)
                {
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
            foreach(var typePartDefinition in source.ContentTypePartDefinitionRecords)
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
            // The serial number is stored in local cache in order to prevent
            // loading the record if it's not necessary

            int serial;
            if (!_memoryCache.TryGetValue(TypeHashCacheKey, out serial))
            {
                serial = _memoryCache.Set(
                    TypeHashCacheKey,
                    (await GetContentDefinitionRecordAsync()).Serial,
                    _signal.GetToken(TypeHashCacheKey)
                );
            }

            return serial;
        }

        private async Task<ContentDefinitionRecord> GetContentDefinitionRecordAsync()
        {
            if (_contentDefinitionRecord != null)
            {
                return _contentDefinitionRecord;
            }

            return _contentDefinitionRecord = await _contentDefinitionStore.LoadContentDefinitionAsync();
        }

        private async Task UpdateContentDefinitionRecordAsync()
        {
            _contentDefinitionRecord.Serial++;
            await _contentDefinitionStore.SaveContentDefinitionAsync(_contentDefinitionRecord);

            _signal.SignalToken(TypeHashCacheKey);


            // Release cached values
            _typeDefinitions.Clear();
            _partDefinitions.Clear();
        }
    }
}
