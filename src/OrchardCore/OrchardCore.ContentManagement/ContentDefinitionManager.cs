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
using OrchardCore.ContentManagement.Metadata.Documents;
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
                var contentTypeDefinitionDocument = LoadContentDefinitionDocument()
                    .ContentTypeDefinitions
                    .FirstOrDefault(x => x.Name == name);

                _scopedTypeDefinitions[name] = typeDefinition = Build(contentTypeDefinitionDocument, LoadContentDefinitionDocument().ContentPartDefinitions);
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
                var contentTypeDefinitionDocument = GetContentDefinitionDocument()
                    .ContentTypeDefinitions
                    .FirstOrDefault(x => x.Name == name);

                return Build(contentTypeDefinitionDocument, GetContentDefinitionDocument().ContentPartDefinitions);
            });
        }

        public ContentPartDefinition LoadPartDefinition(string name)
        {
            if (!_scopedPartDefinitions.TryGetValue(name, out var partDefinition))
            {
                _scopedPartDefinitions[name] = partDefinition = Build(LoadContentDefinitionDocument()
                    .ContentPartDefinitions
                    .FirstOrDefault(x => x.Name == name));
            };

            return partDefinition;
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            return _cachedPartDefinitions.GetOrAdd(name, n =>
            {
                return Build(GetContentDefinitionDocument()
                    .ContentPartDefinitions
                    .FirstOrDefault(x => x.Name == name));
            });
        }

        public IEnumerable<ContentTypeDefinition> LoadTypeDefinitions()
        {
            return LoadContentDefinitionDocument().ContentTypeDefinitions.Select(x => LoadTypeDefinition(x.Name)).ToList();
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return GetContentDefinitionDocument().ContentTypeDefinitions.Select(x => GetTypeDefinition(x.Name)).ToList();
        }

        public IEnumerable<ContentPartDefinition> LoadPartDefinitions()
        {
            return LoadContentDefinitionDocument().ContentPartDefinitions.Select(x => LoadPartDefinition(x.Name)).ToList();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions()
        {
            return GetContentDefinitionDocument().ContentPartDefinitions.Select(x => GetPartDefinition(x.Name)).ToList();
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        {
            Apply(contentTypeDefinition, Acquire(contentTypeDefinition));
            UpdateContentDefinitionDocument();
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition)
        {
            Apply(contentPartDefinition, Acquire(contentPartDefinition));
            UpdateContentDefinitionDocument();
        }

        public void DeleteTypeDefinition(string name)
        {
            var record = LoadContentDefinitionDocument().ContentTypeDefinitions.FirstOrDefault(x => x.Name == name);

            // deletes the content type record associated
            if (record != null)
            {
                LoadContentDefinitionDocument().ContentTypeDefinitions.Remove(record);
                UpdateContentDefinitionDocument();
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
            var record = LoadContentDefinitionDocument().ContentPartDefinitions.FirstOrDefault(x => x.Name == name);

            if (record != null)
            {
                LoadContentDefinitionDocument().ContentPartDefinitions.Remove(record);
                UpdateContentDefinitionDocument();
            }
        }

        private ContentTypeDefinitionDocument Acquire(ContentTypeDefinition contentTypeDefinition)
        {
            var result = LoadContentDefinitionDocument().ContentTypeDefinitions.FirstOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                result = new ContentTypeDefinitionDocument { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                LoadContentDefinitionDocument().ContentTypeDefinitions.Add(result);
            }
            return result;
        }

        private ContentPartDefinitionDocument Acquire(ContentPartDefinition contentPartDefinition)
        {
            var result = LoadContentDefinitionDocument().ContentPartDefinitions.FirstOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null)
            {
                result = new ContentPartDefinitionDocument { Name = contentPartDefinition.Name, };
                LoadContentDefinitionDocument().ContentPartDefinitions.Add(result);
            }
            return result;
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionDocument record)
        {
            record.DisplayName = model.DisplayName;
            record.Settings = model.Settings;

            var toRemove = record.ContentTypePartDefinitions
                .Where(typePartDefinitionDocument => !model.Parts.Any(part => typePartDefinitionDocument.Name == part.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitions.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartRecord = record.ContentTypePartDefinitions.FirstOrDefault(r => r.Name == part.Name);
                if (typePartRecord == null)
                {
                    typePartRecord = new ContentTypePartDefinitionDocument
                    {
                        PartName = part.PartDefinition.Name,
                        Name = part.Name,
                        Settings = part.Settings
                    };

                    record.ContentTypePartDefinitions.Add(typePartRecord);
                }
                Apply(part, typePartRecord);
            }
        }

        private void Apply(ContentTypePartDefinition model, ContentTypePartDefinitionDocument record)
        {
            record.Settings = model.Settings;
        }

        private void Apply(ContentPartDefinition model, ContentPartDefinitionDocument record)
        {
            record.Settings = model.Settings;

            var toRemove = record.ContentPartFieldDefinitions
                .Where(partFieldDefinitionDocument => !model.Fields.Any(partField => partFieldDefinitionDocument.Name == partField.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentPartFieldDefinitions.Remove(remove);
            }

            foreach (var field in model.Fields)
            {
                var fieldName = field.Name;
                var partFieldRecord = record.ContentPartFieldDefinitions.FirstOrDefault(r => r.Name == fieldName);
                if (partFieldRecord == null)
                {
                    partFieldRecord = new ContentPartFieldDefinitionDocument
                    {
                        FieldName = field.FieldDefinition.Name,
                        Name = field.Name
                    };
                    record.ContentPartFieldDefinitions.Add(partFieldRecord);
                }
                Apply(field, partFieldRecord);
            }
        }

        private void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionDocument record)
        {
            record.Settings = model.Settings;
        }

        ContentTypeDefinition Build(ContentTypeDefinitionDocument source, IList<ContentPartDefinitionDocument> partDefinitionDocuments)
        {
            if (source == null)
            {
                return null;
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitions.Select(tp => Build(tp, partDefinitionDocuments.FirstOrDefault(p => p.Name == tp.PartName))),
                source.Settings);

            return contentTypeDefinition;
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionDocument source, ContentPartDefinitionDocument partDefinitionDocument)
        {
            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionDocument) ?? new ContentPartDefinition(source.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), new JObject()),
                source.Settings);
        }

        ContentPartDefinition Build(ContentPartDefinitionDocument source)
        {
            return source == null ? null : new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitions.Select(Build),
                source.Settings);
        }

        ContentPartFieldDefinition Build(ContentPartFieldDefinitionDocument source)
        {
            return source == null ? null : new ContentPartFieldDefinition(
                Build(new ContentFieldDefinitionDocument { Name = source.FieldName }),
                source.Name,
                source.Settings
            );
        }

        ContentFieldDefinition Build(ContentFieldDefinitionDocument source)
        {
            return source == null ? null : new ContentFieldDefinition(source.Name);
        }

        public Task<int> GetTypesHashAsync()
        {
            return Task.FromResult(GetContentDefinitionDocument().Serial);
        }

        /// <summary>
        /// Returns the document from the store to be updated.
        /// </summary>
        private ContentDefinitionDocument LoadContentDefinitionDocument() =>
            _contentDefinitionStore.LoadContentDefinitionAsync().GetAwaiter().GetResult();

        private ContentDefinitionDocument GetContentDefinitionDocument()
        {
            if (!_memoryCache.TryGetValue<ContentDefinitionDocument>(CacheKey, out var record))
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

        private void UpdateContentDefinitionDocument()
        {
            var contentDefinitionDocument = LoadContentDefinitionDocument();

            contentDefinitionDocument.Serial++;
            _contentDefinitionStore.SaveContentDefinitionAsync(contentDefinitionDocument).GetAwaiter().GetResult();

            // Cache invalidation at the end of the scope.
            _signal.DeferredSignalToken(CacheKey);

            // If multiple updates in the same scope, types and parts may need to be rebuilt.
            _scopedTypeDefinitions.Clear();
            _scopedPartDefinitions.Clear();
        }
    }
}
