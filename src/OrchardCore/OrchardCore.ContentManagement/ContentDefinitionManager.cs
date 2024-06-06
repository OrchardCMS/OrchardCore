using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Data.Documents;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private const string CacheKey = nameof(ContentDefinitionManager);

        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly IMemoryCache _memoryCache;

        public ContentDefinitionManager(
            IContentDefinitionStore contentDefinitionStore,
            IMemoryCache memoryCache)
        {
            _contentDefinitionStore = contentDefinitionStore;
            _memoryCache = memoryCache;

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

        public async Task<string> GetIdentifierAsync() => (await _contentDefinitionStore.GetContentDefinitionAsync()).Identifier;

        private static ContentTypeDefinition LoadTypeDefinition(ContentDefinitionRecord document, string name) => Build(
                document.ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name.EqualsOrdinalIgnoreCase(name)),
                document.ContentPartDefinitionRecords);

        private static ContentTypeDefinition GetTypeDefinition(ContentDefinitionRecord document, string name) =>
           Build(
                document.ContentTypeDefinitionRecords.FirstOrDefault(type => type.Name.EqualsOrdinalIgnoreCase(name)),
                document.ContentPartDefinitionRecords);

        private static ContentPartDefinition LoadPartDefinition(ContentDefinitionRecord document, string name) =>
           Build(document.ContentPartDefinitionRecords.FirstOrDefault(part => part.Name.EqualsOrdinalIgnoreCase(name)));

        private static ContentPartDefinition GetPartDefinition(ContentDefinitionRecord document, string name) => Build(
                document.ContentPartDefinitionRecords.FirstOrDefault(record => record.Name.EqualsOrdinalIgnoreCase(name)));

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
                .ToList();

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
                .ToList();

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

        private static ContentTypeDefinition Build(
            ContentTypeDefinitionRecord source,
            IList<ContentPartDefinitionRecord> partDefinitionRecords) =>
            source is not null
            ? new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(typePart => Build(
                    typePart,
                    partDefinitionRecords.FirstOrDefault(part => part.Name.EqualsOrdinalIgnoreCase(typePart.PartName)))),
                source.Settings)
            : null;

        private static ContentTypePartDefinition Build(
            ContentTypePartDefinitionRecord source,
            ContentPartDefinitionRecord partDefinitionRecord) =>
            source is not null
            ? new ContentTypePartDefinition(
                source.Name,
                Build(partDefinitionRecord) ?? new ContentPartDefinition(source.PartName, [], []),
                source.Settings)
            : null;

        private static ContentPartDefinition Build(ContentPartDefinitionRecord source) =>
            source is not null
            ? new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                source.Settings)
            : null;

        private static ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source) =>
            source is not null
            ? new ContentPartFieldDefinition(
                Build(new ContentFieldDefinitionRecord
                {
                    Name = source.FieldName,
                }),
                source.Name,
                source.Settings)
            : null;

        private static ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
            => source is null ? null : new ContentFieldDefinition(source.Name);

        private async Task UpdateContentDefinitionRecordAsync(ContentDefinitionRecord document)
        {
            await _contentDefinitionStore.SaveContentDefinitionAsync(document);
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
                _memoryCache.Set(CacheKey, cacheEntry);
            }
        }
    }
}
