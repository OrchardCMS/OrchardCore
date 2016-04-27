using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Events;
using Orchard.ContentTypes.ViewModels;
using Orchard.Localization;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Utility;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement;
using YesSql.Core.Services;
using Orchard.ContentManagement.Records;
using Orchard.Events;
using Microsoft.Extensions.Logging;
using Orchard.ContentTypes.Editors;

namespace Orchard.ContentTypes.Services
{
    public class ContentDefinitionService : IContentDefinitionService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IEnumerable<IContentDefinitionEditorEvents> _contentDefinitionEditorEvents;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IEventBus _eventBus;

        public ContentDefinitionService(
                IContentDefinitionManager contentDefinitionManager,
                IContentManager contentManager,
                ISession session,
                IEnumerable<IContentPartDriver> contentPartDrivers,
                IEnumerable<IContentFieldDriver> contentFieldDrivers,
                IEnumerable<IContentDefinitionEditorEvents> contentDefinitionEditorEvents,
                ILogger<IContentDefinitionService> logger,
                IEventBus eventBus)
        {
            _eventBus = eventBus;
            _session = session;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _contentDefinitionEditorEvents = contentDefinitionEditorEvents;

            Logger = logger;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; }
        public Localizer T { get; set; }

        public IEnumerable<EditTypeViewModel> GetTypes()
        {
            return _contentDefinitionManager.ListTypeDefinitions().Select(ctd => new EditTypeViewModel(ctd)).OrderBy(m => m.DisplayName);
        }

        public EditTypeViewModel GetType(string name)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(name);

            if (contentTypeDefinition == null)
                return null;

            var viewModel = new EditTypeViewModel(contentTypeDefinition)
            {
                Templates = _contentDefinitionEditorEvents.Invoke(x => x.TypeEditor(contentTypeDefinition), Logger)
            };

            foreach (var part in viewModel.Parts)
            {
                part._Definition.ContentTypeDefinition = contentTypeDefinition;
                part.Templates = _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditor(part._Definition), Logger);
                foreach (var field in part.PartDefinition.Fields)
                    field.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditor(field._Definition), Logger);
            }

            if (viewModel.Fields.Any())
            {
                foreach (var field in viewModel.Fields)
                    field.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditor(field._Definition), Logger);
            }

            return viewModel;
        }

        public ContentTypeDefinition AddType(string name, string displayName)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("displayName");
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                name = GenerateContentTypeNameFromDisplayName(displayName);
            }
            else {
                if (!name[0].IsLetter())
                {
                    throw new ArgumentException("Content type name must start with a letter", "name");
                }
            }

            while (_contentDefinitionManager.GetTypeDefinition(name) != null)
                name = VersionName(name);

            var contentTypeDefinition = new ContentTypeDefinition(name, displayName);
            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
            _contentDefinitionManager.AlterTypeDefinition(name, cfg => cfg.Creatable().Draftable().Listable().Securable());
            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentTypeCreated(new ContentTypeCreatedContext { ContentTypeDefinition = contentTypeDefinition }));

            return contentTypeDefinition;
        }

        public void AlterType(EditTypeViewModel typeViewModel, IUpdateModel updateModel)
        {
            var updater = new PrefixedModelUpdater(updateModel);
            _contentDefinitionManager.AlterTypeDefinition(typeViewModel.Name, typeBuilder =>
            {
                typeBuilder.DisplayedAs(typeViewModel.DisplayName);

                // allow extensions to alter type configuration
                _contentDefinitionEditorEvents.Invoke(x => x.TypeEditorUpdating(typeBuilder), Logger);
                typeViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.TypeEditorUpdate(typeBuilder, updater), Logger);
                _contentDefinitionEditorEvents.Invoke(x => x.TypeEditorUpdated(typeBuilder), Logger);

                foreach (var part in typeViewModel.Parts)
                {
                    var partViewModel = part;

                    // enable updater to be aware of changing part prefix
                    updater.Prefix = secondHalf => String.Format("{0}.{1}", partViewModel.Prefix, secondHalf);

                    // allow extensions to alter typePart configuration
                    typeBuilder.WithPart(partViewModel.PartDefinition.Name, typePartBuilder =>
                    {
                        _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditorUpdating(typePartBuilder), Logger);
                        partViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditorUpdate(typePartBuilder, updater), Logger);
                        _contentDefinitionEditorEvents.Invoke(x => x.TypePartEditorUpdated(typePartBuilder), Logger);
                    });

                    if (!partViewModel.PartDefinition.Fields.Any())
                        continue;

                    _contentDefinitionManager.AlterPartDefinition(partViewModel.PartDefinition.Name, partBuilder =>
                    {
                        var fieldFirstHalf = String.Format("{0}.{1}", partViewModel.Prefix, partViewModel.PartDefinition.Prefix);
                        foreach (var field in partViewModel.PartDefinition.Fields)
                        {
                            var fieldViewModel = field;

                            // enable updater to be aware of changing field prefix
                            updater.Prefix = secondHalf =>
                                String.Format("{0}.{1}.{2}", fieldFirstHalf, fieldViewModel.Prefix, secondHalf);
                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder =>
                            {
                                _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdating(partFieldBuilder), Logger);
                                fieldViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdate(partFieldBuilder, updater), Logger);
                                _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdated(partFieldBuilder), Logger);
                            });
                        }
                    });
                }

                if (typeViewModel.Fields.Any())
                {
                    _contentDefinitionManager.AlterPartDefinition(typeViewModel.Name, partBuilder =>
                    {
                        foreach (var field in typeViewModel.Fields)
                        {
                            var fieldViewModel = field;

                            // enable updater to be aware of changing field prefix
                            updater.Prefix = secondHalf =>
                                string.Format("{0}.{1}", fieldViewModel.Prefix, secondHalf);

                            // allow extensions to alter partField configuration
                            partBuilder.WithField(fieldViewModel.Name, partFieldBuilder =>
                            {
                                _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdating(partFieldBuilder), Logger);
                                fieldViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdate(partFieldBuilder, updater), Logger);
                                _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdated(partFieldBuilder), Logger);
                            });
                        }
                    });
                }
            });
        }

        public void RemoveType(string name, bool deleteContent)
        {

            // first remove all attached parts
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(name);
            var partDefinitions = typeDefinition.Parts.ToArray();
            foreach (var partDefinition in partDefinitions)
            {
                RemovePartFromType(partDefinition.PartDefinition.Name, name);

                // delete the part if it's its own part
                if (partDefinition.PartDefinition.Name == name)
                {
                    RemovePart(name);
                }
            }

            _contentDefinitionManager.DeleteTypeDefinition(name);

            // TODO: Create a scheduled job to delete the content items
            if (deleteContent)
            {
                var contentItems = _session
                    .QueryAsync<ContentItem, ContentItemIndex>(x => x.ContentType == name)
                    .List().Result;

                foreach (var contentItem in contentItems)
                {
                    _session.Delete(contentItem);
                }
            }
            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentTypeRemoved(new ContentTypeRemovedContext { ContentTypeDefinition = typeDefinition })).Wait();
        }

        public void AddPartToType(string partName, string typeName)
        {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.WithPart(partName));
            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentPartAttached(new ContentPartAttachedContext { ContentTypeName = typeName, ContentPartName = partName })).Wait();
        }

        public void RemovePartFromType(string partName, string typeName)
        {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.RemovePart(partName));
            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentPartDetached(new ContentPartDetachedContext { ContentTypeName = typeName, ContentPartName = partName })).Wait();
        }

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly)
        {
            var typeNames = new HashSet<string>(GetTypes().Select(ctd => ctd.Name));

            // user-defined parts
            // except for those parts with the same name as a type (implicit type's part or a mistake)
            var userContentParts = _contentDefinitionManager.ListPartDefinitions()
                .Where(cpd => !typeNames.Contains(cpd.Name))
                .Select(cpd => new EditPartViewModel(cpd))
                .ToDictionary(
                    k => k.Name,
                    v => v);

            // code-defined parts
            var codeDefinedParts = metadataPartsOnly
                ? Enumerable.Empty<EditPartViewModel>()
                : _contentPartDrivers
                    .Select(d => d.GetPartInfo())
                        .Where(cpd => !userContentParts.ContainsKey(cpd.PartName))
                        .Select(cpi => new EditPartViewModel { Name = cpi.PartName, DisplayName = cpi.PartName })
                    .ToList();

            // Order by display name
            return codeDefinedParts
                .Union(userContentParts.Values)
                .OrderBy(m => m.DisplayName);
        }

        public EditPartViewModel GetPart(string name)
        {
            var contentPartDefinition = _contentDefinitionManager.GetPartDefinition(name);

            if (contentPartDefinition == null)
                return null;

            var viewModel = new EditPartViewModel(contentPartDefinition)
            {
                Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartEditor(contentPartDefinition), Logger)
            };

            return viewModel;
        }

        public EditPartViewModel AddPart(CreatePartViewModel partViewModel)
        {
            var name = partViewModel.Name;

            if (_contentDefinitionManager.GetPartDefinition(name) != null)
                throw new OrchardException(T("Cannot add part named '{0}'. It already exists.", name));

            if (!String.IsNullOrEmpty(name))
            {
                _contentDefinitionManager.AlterPartDefinition(name, builder => builder.Attachable());
                var partDefinition = _contentDefinitionManager.GetPartDefinition(name);
                _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentPartCreated(new ContentPartCreatedContext { ContentPartDefinition = partDefinition })).Wait();
                return new EditPartViewModel(partDefinition);
            }

            return null;
        }

        public void AlterPart(EditPartViewModel partViewModel, IUpdateModel updateModel)
        {
            _contentDefinitionManager.AlterPartDefinition(partViewModel.Name, partBuilder =>
            {
                _contentDefinitionEditorEvents.Invoke(x => x.PartEditorUpdating(partBuilder), Logger);
                partViewModel.Templates = _contentDefinitionEditorEvents.Invoke(x => x.PartEditorUpdate(partBuilder, updateModel), Logger);
                _contentDefinitionEditorEvents.Invoke(x => x.PartEditorUpdated(partBuilder), Logger);
            });
        }

        public void RemovePart(string name)
        {
            var partDefinition = _contentDefinitionManager.GetPartDefinition(name);
            var fieldDefinitions = partDefinition.Fields.ToArray();
            foreach (var fieldDefinition in fieldDefinitions)
            {
                RemoveFieldFromPart(fieldDefinition.Name, name);
            }

            _contentDefinitionManager.DeletePartDefinition(name);
            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentPartRemoved(new ContentPartRemovedContext { ContentPartDefinition = partDefinition })).Wait();
        }

        public IEnumerable<ContentFieldInfo> GetFields()
        {
            return _contentFieldDrivers.Select(d => d.GetFieldInfo());
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName)
        {
            AddFieldToPart(fieldName, fieldName, fieldTypeName, partName);
        }

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName)
        {
            fieldName = fieldName.ToSafeName();
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new OrchardException(T("Fields must have a name containing no spaces or symbols."));
            }
            _contentDefinitionManager.AlterPartDefinition(partName,
                partBuilder => partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName).WithDisplayName(displayName)));

            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentFieldAttached(new ContentFieldAttachedContext
            {
                ContentPartName = partName,
                ContentFieldTypeName = fieldTypeName,
                ContentFieldName = fieldName,
                ContentFieldDisplayName = displayName
            })).Wait();
        }

        public void RemoveFieldFromPart(string fieldName, string partName)
        {
            _contentDefinitionManager.AlterPartDefinition(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
            _eventBus.NotifyAsync<IContentDefinitionEventHandler>(x => x.ContentFieldDetached(new ContentFieldDetachedContext
            {
                ContentPartName = partName,
                ContentFieldName = fieldName
            }));
        }

        public void AlterField(EditPartViewModel partViewModel, EditFieldNameViewModel fieldViewModel)
        {
            _contentDefinitionManager.AlterPartDefinition(partViewModel.Name, partBuilder =>
            {
                partBuilder.WithField(fieldViewModel.Name, fieldBuilder =>
                {
                    _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdating(fieldBuilder), Logger);
                    fieldBuilder.WithDisplayName(fieldViewModel.DisplayName);
                    _contentDefinitionEditorEvents.Invoke(x => x.PartFieldEditorUpdated(fieldBuilder), Logger);
                });
            });
        }

        public string GenerateContentTypeNameFromDisplayName(string displayName)
        {
            displayName = displayName.ToSafeName();

            while (_contentDefinitionManager.GetTypeDefinition(displayName) != null)
                displayName = VersionName(displayName);

            return displayName;
        }

        public string GenerateFieldNameFromDisplayName(string partName, string displayName)
        {
            IEnumerable<ContentPartFieldDefinition> fieldDefinitions;

            var part = _contentDefinitionManager.GetPartDefinition(partName);
            displayName = displayName.ToSafeName();

            if (part == null)
            {
                var type = _contentDefinitionManager.GetTypeDefinition(partName);

                if (type == null)
                {
                    throw new ArgumentException("The part doesn't exist: " + partName);
                }

                var typePart = type.Parts.FirstOrDefault(x => x.PartDefinition.Name == partName);

                // id passed in might be that of a type w/ no implicit field
                if (typePart == null)
                {
                    return displayName;
                }
                else {
                    fieldDefinitions = typePart.PartDefinition.Fields.ToArray();
                }

            }
            else {
                fieldDefinitions = part.Fields.ToArray();
            }

            while (fieldDefinitions.Any(x => String.Equals(displayName.Trim(), x.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                displayName = VersionName(displayName);

            return displayName;
        }

        private static string VersionName(string name)
        {
            int version;
            var nameParts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1 && int.TryParse(nameParts.Last(), out version))
            {
                version = version > 0 ? ++version : 2;
                //this could unintentionally chomp something that looks like a version
                name = string.Join("-", nameParts.Take(nameParts.Length - 1));
            }
            else {
                version = 2;
            }

            return string.Format("{0}-{1}", name, version);
        }
    }
}