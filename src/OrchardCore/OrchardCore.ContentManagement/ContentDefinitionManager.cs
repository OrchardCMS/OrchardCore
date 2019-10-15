using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
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

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Argument cannot be null or empty", nameof(name));
            }

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
                var definition = GetContentDefinitionRecord();
                definition.ContentTypeDefinitionRecords = definition.ContentTypeDefinitionRecords.Remove(record);
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
                var definition = GetContentDefinitionRecord();
                definition.ContentPartDefinitionRecords = definition.ContentPartDefinitionRecords.Remove(record);
                UpdateContentDefinitionRecord();
            }
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition)
        {
            var result = GetContentDefinitionRecord().ContentTypeDefinitionRecords.FirstOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                var definition = GetContentDefinitionRecord();
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                definition.ContentTypeDefinitionRecords = definition.ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition)
        {
            var result = GetContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null)
            {
                var definition = GetContentDefinitionRecord();
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

        ContentTypeDefinition Build(ContentTypeDefinitionRecord source)
        {
            if (source == null)
            {
                return null;
            }

            var typePartDefinitions = new List<ContentTypePartDefinition>();
            foreach (var typePartDefinition in source.ContentTypePartDefinitionRecords)
            {
                typePartDefinitions.Add(Build(typePartDefinition));
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                typePartDefinitions,
                source.Settings);

            return contentTypeDefinition;
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source)
        {
            var partDefinitionRecord = GetContentDefinitionRecord().ContentPartDefinitionRecords.FirstOrDefault(x => x.Name == source.PartName);

            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionRecord) ?? new ContentPartDefinition(source.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), new JObject()),
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

        public Task<int> GetTypesHashAsync()
        {
            return Task.FromResult(GetContentDefinitionRecord().Serial);
        }

        private ContentDefinitionRecord GetContentDefinitionRecord()
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
                var record = _contentDefinitionStore.LoadContentDefinitionAsync().GetAwaiter().GetResult();

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

        private void UpdateContentDefinitionRecord()
        {
            var scopedRecord = ScopedCache.ContentDefinitionRecord;
            scopedRecord.Serial++;

            _contentDefinitionStore.SaveContentDefinitionAsync(scopedRecord).GetAwaiter().GetResult();

            // Cache invalidation e.g after committing the session.
            _signal.DeferredSignalToken(CacheKey);

            // In case of multiple scoped updates, types and parts may need to be rebuilt while
            // in the same scope, so we don't defer the clearing of the related cached results.
            _typeDefinitions.Clear();
            _partDefinitions.Clear();
        }
    }
}
