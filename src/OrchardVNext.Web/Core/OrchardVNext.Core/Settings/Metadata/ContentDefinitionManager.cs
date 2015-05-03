using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OrchardVNext.ContentManagement.MetaData;
using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.ContentManagement.MetaData.Services;
using OrchardVNext.Core.Settings.Metadata.Records;
using OrchardVNext.Data;

namespace OrchardVNext.Core.Settings.Metadata {
    public class ContentDefinitionManager : Component, IContentDefinitionManager {
        private readonly IContentStorageProvider _contentStorageProvider;
        private readonly ISettingsFormatter _settingsFormatter;

        public ContentDefinitionManager(
            IContentStorageProvider contentStorageProvider,
            ISettingsFormatter settingsFormatter) {
            _contentStorageProvider = contentStorageProvider;
            _settingsFormatter = settingsFormatter;
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions() {
            return AcquireContentTypeDefinitions().Values;
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions() {
            return AcquireContentPartDefinitions().Values;
        }

        public IEnumerable<ContentFieldDefinition> ListFieldDefinitions() {
            return AcquireContentFieldDefinitions().Values;
        }

        public ContentTypeDefinition GetTypeDefinition(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return null;
            }

            var contentTypeDefinitions = AcquireContentTypeDefinitions();
            if (contentTypeDefinitions.ContainsKey(name)) {
                return contentTypeDefinitions[name];
            }

            return null;
        }

        public ContentPartDefinition GetPartDefinition(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return null;
            }

            var contentPartDefinitions = AcquireContentPartDefinitions();
            if (contentPartDefinitions.ContainsKey(name)) {
                return contentPartDefinitions[name];
            }

            return null;
        }

        public void DeleteTypeDefinition(string name) {
            var record = _contentStorageProvider
                .Query<ContentTypeDefinitionRecord>(x => x.Name == name)
                .SingleOrDefault();

            // deletes the content type record associated
            if (record != null) {
                _contentStorageProvider.Remove(record);
            }
        }

        public void DeletePartDefinition(string name) {
            // remove parts from current types
            var typesWithPart = ListTypeDefinitions().Where(typeDefinition => typeDefinition.Parts.Any(part => part.PartDefinition.Name == name));

            foreach (var typeDefinition in typesWithPart) {
                this.AlterTypeDefinition(typeDefinition.Name, builder => builder.RemovePart(name));
            }

            // delete part
            var record = _contentStorageProvider
                .Query<ContentPartDefinitionRecord>(x => x.Name == name)
                .SingleOrDefault();

            if (record != null) {
                _contentStorageProvider.Remove(record);
            }
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition) {
            Apply(contentTypeDefinition, Acquire(contentTypeDefinition));
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition) {
            Apply(contentPartDefinition, Acquire(contentPartDefinition));
        }


        private IDictionary<string, ContentTypeDefinition> AcquireContentTypeDefinitions() {
            AcquireContentPartDefinitions();

            var contentTypeDefinitionRecords = _contentStorageProvider
                .Query<ContentTypeDefinitionRecord>()
                .Select(Build);

            return contentTypeDefinitionRecords.ToDictionary(x => x.Name, y => y, StringComparer.OrdinalIgnoreCase);
        }

        private IDictionary<string, ContentPartDefinition> AcquireContentPartDefinitions() {
            var contentPartDefinitionRecords = _contentStorageProvider
                .Query<ContentPartDefinitionRecord>()
                .Select(Build);

            return contentPartDefinitionRecords.ToDictionary(x => x.Name, y => y, StringComparer.OrdinalIgnoreCase);
        }

        private IDictionary<string, ContentFieldDefinition> AcquireContentFieldDefinitions() {
            return _contentStorageProvider
                .Query<ContentFieldDefinitionRecord>()
                .Select(Build)
                .ToDictionary(x => x.Name, y => y);
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition) {
            var result = _contentStorageProvider
                .Query<ContentTypeDefinitionRecord>(x => x.Name == contentTypeDefinition.Name)
                .SingleOrDefault();

            if (result == null) {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                _contentStorageProvider.Store(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition) {
            var result = _contentStorageProvider
                .Query<ContentPartDefinitionRecord>(x => x.Name == contentPartDefinition.Name)
                .SingleOrDefault();

            if (result == null) {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name };
                _contentStorageProvider.Store(result);
            }
            return result;
        }

        private ContentFieldDefinitionRecord Acquire(ContentFieldDefinition contentFieldDefinition) {
            var result = _contentStorageProvider
                .Query<ContentFieldDefinitionRecord>(x => x.Name == contentFieldDefinition.Name)
                .SingleOrDefault();

            if (result == null) {
                result = new ContentFieldDefinitionRecord { Name = contentFieldDefinition.Name };
                _contentStorageProvider.Store(result);
            }
            return result;
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record) {
            record.DisplayName = model.DisplayName;
            record.Settings = _settingsFormatter.Map(model.Settings).ToString();

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(partDefinitionRecord => model.Parts.All(part => partDefinitionRecord.ContentPartDefinitionRecord.Name != part.PartDefinition.Name))
                .ToList();

            foreach (var remove in toRemove) {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts) {
                var partName = part.PartDefinition.Name;
                var typePartRecord = record.ContentTypePartDefinitionRecords.SingleOrDefault(r => r.ContentPartDefinitionRecord.Name == partName);
                if (typePartRecord == null) {
                    typePartRecord = new ContentTypePartDefinitionRecord { ContentPartDefinitionRecord = Acquire(part.PartDefinition) };
                    record.ContentTypePartDefinitionRecords.Add(typePartRecord);
                }
                Apply(part, typePartRecord);
            }
        }

        private void Apply(ContentTypePartDefinition model, ContentTypePartDefinitionRecord record) {
            record.Settings = Compose(_settingsFormatter.Map(model.Settings));
        }

        private void Apply(ContentPartDefinition model, ContentPartDefinitionRecord record) {
            record.Settings = _settingsFormatter.Map(model.Settings).ToString();

            var toRemove = record.ContentPartFieldDefinitionRecords
                .Where(partFieldDefinitionRecord => model.Fields.All(partField => partFieldDefinitionRecord.Name != partField.Name))
                .ToList();

            foreach (var remove in toRemove) {
                record.ContentPartFieldDefinitionRecords.Remove(remove);
            }

            foreach (var field in model.Fields) {
                var fieldName = field.Name;
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.SingleOrDefault(r => r.Name == fieldName);
                if (partFieldRecord == null) {
                    partFieldRecord = new ContentPartFieldDefinitionRecord {
                        ContentFieldDefinitionRecord = Acquire(field.FieldDefinition),
                        Name = field.Name
                    };
                    record.ContentPartFieldDefinitionRecords.Add(partFieldRecord);
                }
                Apply(field, partFieldRecord);
            }
        }

        private void Apply(ContentPartFieldDefinition model, ContentPartFieldDefinitionRecord record) {
            record.Settings = Compose(_settingsFormatter.Map(model.Settings));
        }

        ContentTypeDefinition Build(ContentTypeDefinitionRecord source) {
            return new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(Build),
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source) {
            return new ContentTypePartDefinition(
                Build(source.ContentPartDefinitionRecord),
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentPartDefinition Build(ContentPartDefinitionRecord source) {
            return new ContentPartDefinition(
                source.Name,
                source.ContentPartFieldDefinitionRecords.Select(Build),
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentPartFieldDefinition Build(ContentPartFieldDefinitionRecord source) {
            return new ContentPartFieldDefinition(
                Build(source.ContentFieldDefinitionRecord),
                source.Name,
                _settingsFormatter.Map(Parse(source.Settings)));
        }

        ContentFieldDefinition Build(ContentFieldDefinitionRecord source) {
            return new ContentFieldDefinition(source.Name);
        }

        XElement Parse(string settings) {
            if (string.IsNullOrEmpty(settings))
                return null;

            try {
                return XElement.Parse(settings);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Unable to parse settings xml");
                return null;
            }
        }

        static string Compose(XElement map) {
            return map?.ToString();
        }
    }
}