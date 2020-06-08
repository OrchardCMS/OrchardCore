using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Environment.Cache;

namespace OrchardCore.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string CacheKey = nameof(ContentDefinitionManager);

        private readonly ISignal _signal;
        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly IMemoryCache _memoryCache;

        private readonly ConcurrentDictionary<string, ContentTypeDefinition> _cachedTypeDefinitions;
        private readonly ConcurrentDictionary<string, ContentPartDefinition> _cachedPartDefinitions;

        private readonly Dictionary<string, ContentTypeDefinition> _scopedTypeDefinitions = new Dictionary<string, ContentTypeDefinition>();
        private readonly Dictionary<string, ContentPartDefinition> _scopedPartDefinitions = new Dictionary<string, ContentPartDefinition>();

        public ContentDefinitionManager(
            ISignal signal,
            IContentDefinitionStore contentDefinitionStore,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _contentDefinitionStore = contentDefinitionStore;
            _memoryCache = memoryCache;

            _cachedTypeDefinitions = _memoryCache.GetOrCreate("TypeDefinitions", entry => new ConcurrentDictionary<string, ContentTypeDefinition>());
            _cachedPartDefinitions = _memoryCache.GetOrCreate("PartDefinitions", entry => new ConcurrentDictionary<string, ContentPartDefinition>());
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        public ContentTypeDefinition LoadTypeDefinition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(name));
            }

            if (!_scopedTypeDefinitions.TryGetValue(name, out var typeDefinition))
            {
                var contentTypeDefinitionRecord = LoadContentDefinitionRecord()
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => x.Name == name);

                _scopedTypeDefinitions[name] = typeDefinition = Build(contentTypeDefinitionRecord, LoadContentDefinitionRecord().ContentPartDefinitionRecords);
            };

            return typeDefinition;
        }

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(name));
            }

            return _cachedTypeDefinitions.GetOrAdd(name, n =>
            {
                var contentTypeDefinitionRecord = GetContentDefinitionRecord()
                    .ContentTypeDefinitionRecords
                    .FirstOrDefault(x => x.Name == name);

                return Build(contentTypeDefinitionRecord, GetContentDefinitionRecord().ContentPartDefinitionRecords);
            });
        }

        public ContentPartDefinition LoadPartDefinition(string name)
        {
            if (!_scopedPartDefinitions.TryGetValue(name, out var partDefinition))
            {
                _scopedPartDefinitions[name] = partDefinition = Build(LoadContentDefinitionRecord()
                    .ContentPartDefinitionRecords
                    .FirstOrDefault(x => x.Name == name));
            };

            return partDefinition;
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            return _cachedPartDefinitions.GetOrAdd(name, n =>
            {
                return Build(GetContentDefinitionRecord()
                    .ContentPartDefinitionRecords
                    .FirstOrDefault(x => x.Name == name));
            });
        }

        public IEnumerable<ContentTypeDefinition> LoadTypeDefinitions()
        {
            return LoadContentDefinitionRecord().ContentTypeDefinitionRecords.Select(x => LoadTypeDefinition(x.Name)).ToList();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return GetContentDefinitionRecord().ContentTypeDefinitionRecords.Select(x => GetTypeDefinition(x.Name)).ToList();
        }

        public IEnumerable<ContentPartDefinition> LoadPartDefinitions()
        {
            return LoadContentDefinitionRecord().ContentPartDefinitionRecords.Select(x => LoadPartDefinition(x.Name)).ToList();
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
            var record = LoadContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name == name);

            // deletes the content type record associated
            if (record != null)
            {
                LoadContentDefinitionRecord().ContentTypeDefinitionRecords.Remove(record);
                UpdateContentDefinitionRecord();
            }
        }

        public void DeletePartDefinition(string name)
        {
            // remove parts from current types
            var typesWithPart = LoadTypeDefinitions().Where(typeDefinition => typeDefinition.Parts.Any(part => part.PartDefinition.Name == name));

            foreach (var typeDefinition in typesWithPart)
            {
                this.AlterTypeDefinition(typeDefinition.Name, builder => builder.RemovePart(name));
            }

            // delete part
            var record = LoadContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == name);

            if (record != null)
            {
                LoadContentDefinitionRecord().ContentPartDefinitionRecords.Remove(record);
                UpdateContentDefinitionRecord();
            }
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition)
        {
            var result = LoadContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                LoadContentDefinitionRecord().ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition)
        {
            var result = LoadContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null)
            {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name, };
                LoadContentDefinitionRecord().ContentPartDefinitionRecords.Add(result);
            }
            return result;
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

        private ContentTypeDefinition Build(ContentTypeDefinitionRecord source, IList<ContentPartDefinitionRecord> partDefinitionRecords)
        {
            if (source == null)
            {
                return null;
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(tp => Build(tp, partDefinitionRecords.FirstOrDefault(p => p.Name == tp.PartName))),
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

        private ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
        {
            return source == null ? null : new ContentFieldDefinition(source.Name);
        }

        public Task<int> GetTypesHashAsync()
        {
            return Task.FromResult(GetContentDefinitionRecord().Serial);
        }

        /// <summary>
        /// Returns the document from the store to be updated.
        /// </summary>
        private ContentDefinitionRecord LoadContentDefinitionRecord() =>
            _contentDefinitionStore.LoadContentDefinitionAsync().GetAwaiter().GetResult();

        private ContentDefinitionRecord GetContentDefinitionRecord()
        {
            if (!_memoryCache.TryGetValue<ContentDefinitionRecord>(CacheKey, out var record))
            {
                var changeToken = ChangeToken;

                var typeDefinitions = _cachedTypeDefinitions;
                var partDefinitions = _cachedPartDefinitions;

                // Using local vars prevents the lambda from holding a ref on this scoped service.
                changeToken.RegisterChangeCallback((state) =>
                {
                    typeDefinitions.Clear();
                    partDefinitions.Clear();
                },
                state: null);

                record = _contentDefinitionStore.GetContentDefinitionAsync().GetAwaiter().GetResult();

                _memoryCache.Set(CacheKey, record, changeToken);
            }

            return record;
        }

        private void UpdateContentDefinitionRecord()
        {
            var contentDefinitionRecord = LoadContentDefinitionRecord();

            contentDefinitionRecord.Serial++;
            _contentDefinitionStore.SaveContentDefinitionAsync(contentDefinitionRecord).GetAwaiter().GetResult();

            // Cache invalidation at the end of the scope.
            _signal.DeferredSignalToken(CacheKey);

            // If multiple updates in the same scope, types and parts may need to be rebuilt.
            _scopedTypeDefinitions.Clear();
            _scopedPartDefinitions.Clear();
        }
    }
}
