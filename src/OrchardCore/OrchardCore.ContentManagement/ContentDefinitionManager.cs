using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Documents;
using OrchardCore.Environment.Cache;

namespace OrchardCore.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string TypeHashCacheKey = "ContentDefinitionManager:Serial";

        private ContentDefinitionDocument _contentDefinitionDocument;
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

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            return _typeDefinitions.GetOrAdd(name, n =>
            {
                var contentTypeDefinitionDocument = GetContentDefinitionDocument()
                    .ContentTypeDefinitions
                    .FirstOrDefault(x => x.Name == name);

                return Build(contentTypeDefinitionDocument);
            });
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            return _partDefinitions.GetOrAdd(name, n =>
            {
                return Build(GetContentDefinitionDocument()
                .ContentPartDefinitions
                .FirstOrDefault(x => x.Name == name));
            });
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return GetContentDefinitionDocument().ContentTypeDefinitions.Select(x => GetTypeDefinition(x.Name)).ToList();
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
            var document = GetContentDefinitionDocument().ContentTypeDefinitions.FirstOrDefault(x => x.Name == name);

            // deletes the content type document associated
            if (document != null)
            {
                GetContentDefinitionDocument().ContentTypeDefinitions.Remove(document);
                UpdateContentDefinitionDocument();
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
            var document = GetContentDefinitionDocument().ContentPartDefinitions.FirstOrDefault(x => x.Name == name);

            if (document != null)
            {
                GetContentDefinitionDocument().ContentPartDefinitions.Remove(document);
                UpdateContentDefinitionDocument();
            }
        }

        private ContentTypeDefinitionDocument Acquire(ContentTypeDefinition contentTypeDefinition)
        {
            var result = GetContentDefinitionDocument().ContentTypeDefinitions.FirstOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                result = new ContentTypeDefinitionDocument { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                GetContentDefinitionDocument().ContentTypeDefinitions.Add(result);
            }
            return result;
        }

        private ContentPartDefinitionDocument Acquire(ContentPartDefinition contentPartDefinition)
        {
            var result = GetContentDefinitionDocument().ContentPartDefinitions.FirstOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null)
            {
                result = new ContentPartDefinitionDocument { Name = contentPartDefinition.Name, };
                GetContentDefinitionDocument().ContentPartDefinitions.Add(result);
            }
            return result;
        }

        private ContentFieldDefinitionDocument Acquire(ContentFieldDefinition contentFieldDefinition)
        {
            return new ContentFieldDefinitionDocument { Name = contentFieldDefinition.Name };
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionDocument document)
        {
            document.DisplayName = model.DisplayName;
            document.Settings = model.Settings;

            var toRemove = document.ContentTypePartDefinitions
                .Where(typePartDefinitionDocument => !model.Parts.Any(part => typePartDefinitionDocument.Name == part.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                document.ContentTypePartDefinitions.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var typePartDocument = document.ContentTypePartDefinitions.FirstOrDefault(r => r.Name == part.Name);
                if (typePartDocument == null)
                {
                    typePartDocument = new ContentTypePartDefinitionDocument
                    {
                        PartName = part.PartDefinition.Name,
                        Name = part.Name,
                        Settings = part.Settings
                    };

                    document.ContentTypePartDefinitions.Add(typePartDocument);
                }
                Apply(part, typePartDocument);
            }

            // Persist changes
            UpdateContentDefinitionDocument();
        }

        private void Apply(ContentTypePartDefinition model, ContentTypePartDefinitionDocument document)
        {
            document.Settings = model.Settings;
        }

        private void Apply(ContentPartDefinition model, ContentPartDefinitionDocument document)
        {
            document.Settings = model.Settings;

            var toRemove = document.ContentPartFieldDefinitions
                .Where(partFieldDefinitionDocument => !model.Fields.Any(partField => partFieldDefinitionDocument.Name == partField.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                document.ContentPartFieldDefinitions.Remove(remove);
            }

            foreach (var field in model.Fields)
            {
                var fieldName = field.Name;
                var partFieldDocument = document.ContentPartFieldDefinitions.FirstOrDefault(r => r.Name == fieldName);
                if (partFieldDocument == null)
                {
                    partFieldDocument = new ContentPartFieldDefinitionDocument
                    {
                        FieldName = field.FieldDefinition.Name,
                        Name = field.Name
                    };
                    document.ContentPartFieldDefinitions.Add(partFieldDocument);
                }
                Apply(field, partFieldDocument);
            }
        }

        private void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionDocument document)
        {
            document.Settings = model.Settings;
        }

        ContentTypeDefinition Build(ContentTypeDefinitionDocument source)
        {
            if (source == null)
            {
                return null;
            }

            var contentTypeDefinition = new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitions.Select(Build),
                source.Settings);

            return contentTypeDefinition;
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionDocument source)
        {
            var partDefinitionDocument = GetContentDefinitionDocument().ContentPartDefinitions.FirstOrDefault(x => x.Name == source.PartName);

            return source == null ? null : new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionDocument) ?? new ContentPartDefinition(source.PartName, Enumerable.Empty<ContentPartFieldDefinition>(), new Newtonsoft.Json.Linq.JObject()),
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
            // The serial number is stored in local cache in order to prevent
            // loading the document if it's not necessary

            int serial;
            if (!_memoryCache.TryGetValue(TypeHashCacheKey, out serial))
            {
                serial = _memoryCache.Set(
                    TypeHashCacheKey,
                    GetContentDefinitionDocument().Serial,
                    _signal.GetToken(TypeHashCacheKey)
                );
            }

            return Task.FromResult(serial);
        }

        private ContentDefinitionDocument GetContentDefinitionDocument()
        {
            if (_contentDefinitionDocument != null)
            {
                return _contentDefinitionDocument;
            }

            return _contentDefinitionDocument = _contentDefinitionStore.LoadContentDefinitionAsync().GetAwaiter().GetResult();
        }

        private void UpdateContentDefinitionDocument()
        {
            _contentDefinitionDocument.Serial++;
            _contentDefinitionStore.SaveContentDefinitionAsync(_contentDefinitionDocument).GetAwaiter().GetResult();

            _signal.SignalToken(TypeHashCacheKey);


            // Release cached values
            _typeDefinitions.Clear();
            _partDefinitions.Clear();
        }

    }
}