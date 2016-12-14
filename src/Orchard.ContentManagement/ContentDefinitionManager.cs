using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Orchard.ContentManagement.Metadata.Records;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.Environment.Cache;
using YesSql.Core.Services;

namespace Orchard.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string TypeHashCacheKey = "ContentDefinitionManager:Serial";

        private readonly ISession _session;
        private ContentDefinitionRecord _contentDefinitionRecord;
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;

        private readonly ConcurrentDictionary<string, ContentTypeDefinition> _typeDefinitions;
        private readonly ConcurrentDictionary<string, ContentPartDefinition> _partDefinitions;

        public ContentDefinitionManager(
            ISession session,
            IMemoryCache memoryCache,
            ISignal signal)
        {
            _signal = signal;
            _memoryCache = memoryCache;
            _session = session;

            _typeDefinitions = _memoryCache.GetOrCreate("TypeDefinitions", entry => new ConcurrentDictionary<string, ContentTypeDefinition>());
            _partDefinitions = _memoryCache.GetOrCreate("PartDefinitions", entry => new ConcurrentDictionary<string, ContentPartDefinition>());
        }

        private ContentDefinitionRecord GetContentDefinitionRecord()
        {
            // cache in the current work context
            if (_contentDefinitionRecord != null)
            {
                return _contentDefinitionRecord;
            }

            _contentDefinitionRecord = _session
                .QueryAsync<ContentDefinitionRecord>()
                .FirstOrDefault()
                .Result;

            if (_contentDefinitionRecord == null)
            {
                _contentDefinitionRecord = new ContentDefinitionRecord();
                UpdateContentDefinitionRecord();
            }

            return _contentDefinitionRecord;
        }

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            return _typeDefinitions.GetOrAdd(name, n =>
            {
                var contentTypeDefinitionRecord = GetContentDefinitionRecord()
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => x.Name == name);

                return Build(contentTypeDefinitionRecord);
            });
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            return _partDefinitions.GetOrAdd(name, n =>
            {
                return Build(GetContentDefinitionRecord()
                .ContentPartDefinitionRecords
                .FirstOrDefault(x => x.Name == name));
            });
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return GetContentDefinitionRecord().ContentTypeDefinitionRecords.Select(x => GetTypeDefinition(x.Name)).ToList();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions()
        {
            return GetContentDefinitionRecord().ContentPartDefinitionRecords.Select(x => GetPartDefinition(x.Name)).ToList();
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        {
            Apply(contentTypeDefinition, Acquire(contentTypeDefinition));
            UpdateContentDefinitionRecord();
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition)
        {
            Apply(contentPartDefinition, Acquire(contentPartDefinition));
            UpdateContentDefinitionRecord();
        }

        public void DeleteTypeDefinition(string name)
        {
            var record = GetContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name == name);

            // deletes the content type record associated
            if (record != null)
            {
                GetContentDefinitionRecord().ContentTypeDefinitionRecords.Remove(record);
                UpdateContentDefinitionRecord();
            }
        }

        public void DeletePartDefinition(string name)
        {
            // remove parts from current types
            var typesWithPart = ListTypeDefinitions().Where(typeDefinition => typeDefinition.Parts.Any(part => part.PartDefinition.Name == name));

            foreach (var typeDefinition in typesWithPart)
            {
                this.AlterTypeDefinition(typeDefinition.Name, builder => builder.RemovePart(name));
            }

            // delete part
            var record = GetContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == name);

            if (record != null)
            {
                GetContentDefinitionRecord().ContentPartDefinitionRecords.Remove(record);
                UpdateContentDefinitionRecord();
            }
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition)
        {
            var result = GetContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                GetContentDefinitionRecord().ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition)
        {
            var result = GetContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null)
            {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name, };
                GetContentDefinitionRecord().ContentPartDefinitionRecords.Add(result);
            }
            return result;
        }

        private ContentFieldDefinitionRecord Acquire(ContentFieldDefinition contentFieldDefinition)
        {
            return new ContentFieldDefinitionRecord { Name = contentFieldDefinition.Name };
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record)
        {
            record.DisplayName = model.DisplayName;
            record.Settings = model.Settings;

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(typePartDefinitionRecord => !model.Parts.Any(part => typePartDefinitionRecord.Name == part.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartRecord = record.ContentTypePartDefinitionRecords.FirstOrDefault(r => r.Name == part.Name);
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
            UpdateContentDefinitionRecord();
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

        ContentTypeDefinition Build(ContentTypeDefinitionRecord source)
        {
            if(source == null)
            {
                return null;
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(Build),
                source.Settings);

            return contentTypeDefinition;
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source)
        {
            var partDefinitionRecord = GetContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == source.PartName);

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

        private void UpdateContentDefinitionRecord()
        {
            _contentDefinitionRecord.Serial++;
            _session.Save(_contentDefinitionRecord);
            _signal.SignalToken(TypeHashCacheKey);

            // Release cached values
            _typeDefinitions.Clear();
            _partDefinitions.Clear();
        }

        public Task<int> GetTypesHashAsync()
        {
            // The serial number is store in local cache in order to prevent
            // loading the record if it's not necessary

            int serial;
            if (!_memoryCache.TryGetValue(TypeHashCacheKey, out serial))
            {
                serial = _memoryCache.Set(
                    TypeHashCacheKey,
                    GetContentDefinitionRecord().Serial,
                    _signal.GetToken(TypeHashCacheKey)
                );
            }

            return Task.FromResult(serial);
        }
    }
}