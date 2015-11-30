using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using YesSql.Core.Services;
using Orchard.Core.Settings.Metadata.Records;
using Newtonsoft.Json;

namespace Orchard.Core.Settings.Metadata
{
    public class ContentDefinitionManager : IContentDefinitionManager
    {
        private readonly ISession _session;
        private ContentDefinitionRecord _contentDefinitionRecord;

        public ContentDefinitionManager(ISession session)
        {
            _session = session;
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
                _session.Save(_contentDefinitionRecord);
            }

            return _contentDefinitionRecord;
        }

        public ContentTypeDefinition GetTypeDefinition(string name)
        {
            return Build(GetContentDefinitionRecord()
                .ContentTypeDefinitionRecords
                .FirstOrDefault(x => x.Name == name));
        }

        public ContentPartDefinition GetPartDefinition(string name)
        {
            return Build(GetContentDefinitionRecord()
                .ContentPartDefinitionRecords
                .FirstOrDefault(x => x.Name == name));
        }

        public IEnumerable<ContentTypeDefinition> ListTypeDefinitions()
        {
            return GetContentDefinitionRecord().ContentTypeDefinitionRecords.Where(x => !x.Hidden).Select(Build).ToList();
        }

        public IEnumerable<ContentPartDefinition> ListPartDefinitions()
        {
            return GetContentDefinitionRecord().ContentPartDefinitionRecords.Where(x => !x.Hidden).Select(Build).ToList();
        }

        public IEnumerable<ContentFieldDefinition> ListFieldDefinitions()
        {
            return GetContentDefinitionRecord().ContentFieldDefinitionRecords.OrderBy(x => x.Name).Select(Build).ToList();
        }

        public void StoreTypeDefinition(ContentTypeDefinition contentTypeDefinition)
        {
            Apply(contentTypeDefinition, Acquire(contentTypeDefinition));
        }

        public void StorePartDefinition(ContentPartDefinition contentPartDefinition)
        {
            Apply(contentPartDefinition, Acquire(contentPartDefinition));
        }

        public void DeleteTypeDefinition(string name)
        {
            var record = GetContentDefinitionRecord().ContentTypeDefinitionRecords.SingleOrDefault(x => x.Name == name);

            // deletes the content type record associated
            if (record != null)
            {
                GetContentDefinitionRecord().ContentTypeDefinitionRecords.Remove(record);
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
            var record = GetContentDefinitionRecord().ContentPartDefinitionRecords.SingleOrDefault(x => x.Name == name);

            if (record != null)
            {
                GetContentDefinitionRecord().ContentPartDefinitionRecords.Remove(record);
            }
        }

        private ContentTypeDefinitionRecord Acquire(ContentTypeDefinition contentTypeDefinition)
        {
            var result = GetContentDefinitionRecord().ContentTypeDefinitionRecords.SingleOrDefault(x => x.Name == contentTypeDefinition.Name);
            if (result == null)
            {
                result = new ContentTypeDefinitionRecord { Name = contentTypeDefinition.Name, DisplayName = contentTypeDefinition.DisplayName };
                GetContentDefinitionRecord().ContentTypeDefinitionRecords.Add(result);
            }
            return result;
        }

        private ContentPartDefinitionRecord Acquire(ContentPartDefinition contentPartDefinition)
        {
            var result = GetContentDefinitionRecord().ContentPartDefinitionRecords.SingleOrDefault(x => x.Name == contentPartDefinition.Name);
            if (result == null)
            {
                result = new ContentPartDefinitionRecord { Name = contentPartDefinition.Name, };
                GetContentDefinitionRecord().ContentPartDefinitionRecords.Add(result);
            }
            return result;
        }

        private ContentFieldDefinitionRecord Acquire(ContentFieldDefinition contentFieldDefinition)
        {
            var result = GetContentDefinitionRecord().ContentFieldDefinitionRecords.SingleOrDefault(x => x.Name == contentFieldDefinition.Name);
            if (result == null)
            {
                result = new ContentFieldDefinitionRecord { Name = contentFieldDefinition.Name };
                GetContentDefinitionRecord().ContentFieldDefinitionRecords.Add(result);
            }
            return result;
        }

        private void Apply(ContentTypeDefinition model, ContentTypeDefinitionRecord record)
        {
            record.DisplayName = model.DisplayName;
            record.Settings = model.Settings;

            var toRemove = record.ContentTypePartDefinitionRecords
                .Where(partDefinitionRecord => !model.Parts.Any(part => partDefinitionRecord.ContentPartDefinitionRecord.Name == part.PartDefinition.Name))
                .ToList();

            foreach (var remove in toRemove)
            {
                record.ContentTypePartDefinitionRecords.Remove(remove);
            }

            foreach (var part in model.Parts)
            {
                var partName = part.PartDefinition.Name;
                var typePartRecord = record.ContentTypePartDefinitionRecords.SingleOrDefault(r => r.ContentPartDefinitionRecord.Name == partName);
                if (typePartRecord == null)
                {
                    typePartRecord = new ContentTypePartDefinitionRecord { ContentPartDefinitionRecord = Acquire(part.PartDefinition) };
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
                var partFieldRecord = record.ContentPartFieldDefinitionRecords.SingleOrDefault(r => r.Name == fieldName);
                if (partFieldRecord == null)
                {
                    partFieldRecord = new ContentPartFieldDefinitionRecord
                    {
                        ContentFieldDefinitionRecord = Acquire(field.FieldDefinition),
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
            return source == null ? null : new ContentTypeDefinition(
                source.Name,
                source.DisplayName,
                source.ContentTypePartDefinitionRecords.Select(Build),
                source.Settings);
        }

        ContentTypePartDefinition Build(ContentTypePartDefinitionRecord source)
        {
            return source == null ? null : new ContentTypePartDefinition(
                Build(source.ContentPartDefinitionRecord),
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
                Build(source.ContentFieldDefinitionRecord),
                source.Name,
                source.Settings
            );
        }

        ContentFieldDefinition Build(ContentFieldDefinitionRecord source)
        {
            return source == null ? null : new ContentFieldDefinition(source.Name);
        }
    }
}