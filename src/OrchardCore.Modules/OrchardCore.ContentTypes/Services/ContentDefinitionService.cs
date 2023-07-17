using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentTypes.Services
{
    public class ContentDefinitionService : IContentDefinitionService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentDefinitionEventHandler> _contentDefinitionEventHandlers;
        private readonly IEnumerable<Type> _contentPartTypes;
        private readonly IEnumerable<Type> _contentFieldTypes;
        protected readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public ContentDefinitionService(
                IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers,
                IEnumerable<ContentPart> contentParts,
                IEnumerable<ContentField> contentFields,
                IOptions<ContentOptions> contentOptions,
                ILogger<IContentDefinitionService> logger,
                IStringLocalizer<ContentDefinitionService> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionEventHandlers = contentDefinitionEventHandlers;

            foreach (var element in contentParts.Select(x => x.GetType()))
            {
                logger.LogWarning("The content part '{ContentPart}' should not be registerd in DI. Use AddContentPart<T> instead.", element);
            }

            foreach (var element in contentFields.Select(x => x.GetType()))
            {
                logger.LogWarning("The content field '{ContentField}' should not be registerd in DI. Use AddContentField<T> instead.", element);
            }

            // TODO: This code can be removed in a future release and rationalized to only use ContentPartOptions.
            _contentPartTypes = contentParts.Select(cp => cp.GetType())
                .Union(contentOptions.Value.ContentPartOptions.Select(cpo => cpo.Type));

            // TODO: This code can be removed in a future release and rationalized to only use ContentFieldOptions.
            _contentFieldTypes = contentFields.Select(cf => cf.GetType())
                .Union(contentOptions.Value.ContentFieldOptions.Select(cfo => cfo.Type));

            _logger = logger;
            S = localizer;
        }

        public IEnumerable<EditTypeViewModel> LoadTypes()
        {
            return _contentDefinitionManager
                .LoadTypeDefinitions()
                .Select(ctd => new EditTypeViewModel(ctd))
                .OrderBy(m => m.DisplayName);
        }

        public IEnumerable<EditTypeViewModel> GetTypes()
        {
            return _contentDefinitionManager
                .ListTypeDefinitions()
                .Select(ctd => new EditTypeViewModel(ctd))
                .OrderBy(m => m.DisplayName);
        }

        public EditTypeViewModel LoadType(string name)
        {
            var contentTypeDefinition = _contentDefinitionManager.LoadTypeDefinition(name);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            return new EditTypeViewModel(contentTypeDefinition);
        }

        public EditTypeViewModel GetType(string name)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(name);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            return new EditTypeViewModel(contentTypeDefinition);
        }

        public ContentTypeDefinition AddType(string name, string displayName)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("The 'displayName' can't be null or empty.", nameof(displayName));
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                name = GenerateContentTypeNameFromDisplayName(displayName);
            }
            else
            {
                if (!name[0].IsLetter())
                {
                    throw new ArgumentException("Content type name must start with a letter", nameof(name));
                }
                if (!String.Equals(name, name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Content type name contains invalid characters", nameof(name));
                }
            }

            while (_contentDefinitionManager.LoadTypeDefinition(name) != null)
            {
                name = VersionName(name);
            }

            var contentTypeDefinition = new ContentTypeDefinition(name, displayName);

            _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);

            // Ensure it has its own part.
            _contentDefinitionManager.AlterTypeDefinition(name, builder => builder.WithPart(name));
            _contentDefinitionManager.AlterTypeDefinition(name, cfg => cfg.Creatable().Draftable().Versionable().Listable().Securable());

            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentTypeCreated(context), new ContentTypeCreatedContext { ContentTypeDefinition = contentTypeDefinition }, _logger);

            return contentTypeDefinition;
        }

        public void RemoveType(string name, bool deleteContent)
        {
            // First remove all attached parts.
            var typeDefinition = _contentDefinitionManager.LoadTypeDefinition(name);
            var partDefinitions = typeDefinition.Parts.ToArray();
            foreach (var partDefinition in partDefinitions)
            {
                RemovePartFromType(partDefinition.PartDefinition.Name, name);

                // Delete the part if it's its own part.
                if (partDefinition.PartDefinition.Name == name)
                {
                    RemovePart(name);
                }
            }

            _contentDefinitionManager.DeleteTypeDefinition(name);

            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentTypeRemoved(context), new ContentTypeRemovedContext { ContentTypeDefinition = typeDefinition }, _logger);
        }

        public void AddPartToType(string partName, string typeName)
        {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.WithPart(partName));
            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartAttached(context), new ContentPartAttachedContext { ContentTypeName = typeName, ContentPartName = partName }, _logger);
        }

        public void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName)
        {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.WithPart(name, partName, cfg =>
            {
                cfg.WithDisplayName(displayName);
                cfg.WithDescription(description);
            }));

            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartAttached(context), new ContentPartAttachedContext { ContentTypeName = typeName, ContentPartName = partName }, _logger);
        }

        public void RemovePartFromType(string partName, string typeName)
        {
            _contentDefinitionManager.AlterTypeDefinition(typeName, typeBuilder => typeBuilder.RemovePart(partName));
            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartDetached(context), new ContentPartDetachedContext { ContentTypeName = typeName, ContentPartName = partName }, _logger);
        }

        public IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly)
        {
            var typeNames = new HashSet<string>(LoadTypes().Select(ctd => ctd.Name));

            // User-defined parts.
            // Except for those parts with the same name as a type (implicit type's part or a mistake).
            var userContentParts = _contentDefinitionManager.LoadPartDefinitions()
                .Where(cpd => !typeNames.Contains(cpd.Name))
                .Select(cpd => new EditPartViewModel(cpd))
                .ToDictionary(
                    k => k.Name,
                    v => v);

            // Code-defined parts.
            var codeDefinedParts = metadataPartsOnly
                ? Enumerable.Empty<EditPartViewModel>()
                : _contentPartTypes
                        .Where(cpd => !userContentParts.ContainsKey(cpd.Name))
                        .Select(cpi => new EditPartViewModel { Name = cpi.Name, DisplayName = cpi.Name })
                    .ToList();

            // Order by display name.
            return codeDefinedParts
                .Union(userContentParts.Values)
                .OrderBy(m => m.DisplayName);
        }

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly)
        {
            var typeNames = new HashSet<string>(GetTypes().Select(ctd => ctd.Name));

            // User-defined parts.
            // Except for those parts with the same name as a type (implicit type's part or a mistake).
            var userContentParts = _contentDefinitionManager.ListPartDefinitions()
                .Where(cpd => !typeNames.Contains(cpd.Name))
                .Select(cpd => new EditPartViewModel(cpd))
                .ToDictionary(
                    k => k.Name,
                    v => v);

            // Code-defined parts.
            var codeDefinedParts = metadataPartsOnly
                ? Enumerable.Empty<EditPartViewModel>()
                : _contentPartTypes
                        .Where(cpd => !userContentParts.ContainsKey(cpd.Name))
                        .Select(cpi => new EditPartViewModel { Name = cpi.Name, DisplayName = cpi.Name })
                    .ToList();

            // Order by display name.
            return codeDefinedParts
                .Union(userContentParts.Values)
                .OrderBy(m => m.DisplayName);
        }

        public EditPartViewModel LoadPart(string name)
        {
            var contentPartDefinition = _contentDefinitionManager.LoadPartDefinition(name);

            if (contentPartDefinition == null)
            {
                var contentTypeDefinition = _contentDefinitionManager.LoadTypeDefinition(name);

                if (contentTypeDefinition == null)
                {
                    return null;
                }

                contentPartDefinition = new ContentPartDefinition(name);
            }

            var viewModel = new EditPartViewModel(contentPartDefinition);

            return viewModel;
        }

        public EditPartViewModel GetPart(string name)
        {
            var contentPartDefinition = _contentDefinitionManager.GetPartDefinition(name);

            if (contentPartDefinition == null)
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(name);

                if (contentTypeDefinition == null)
                {
                    return null;
                }

                contentPartDefinition = new ContentPartDefinition(name);
            }

            var viewModel = new EditPartViewModel(contentPartDefinition);

            return viewModel;
        }

        public EditPartViewModel AddPart(CreatePartViewModel partViewModel)
        {
            var name = partViewModel.Name;

            if (_contentDefinitionManager.LoadPartDefinition(name) != null)
                throw new Exception(S["Cannot add part named '{0}'. It already exists.", name]);

            if (!String.IsNullOrEmpty(name))
            {
                _contentDefinitionManager.AlterPartDefinition(name, builder => builder.Attachable());
                var partDefinition = _contentDefinitionManager.LoadPartDefinition(name);
                _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartCreated(context), new ContentPartCreatedContext { ContentPartDefinition = partDefinition }, _logger);
                return new EditPartViewModel(partDefinition);
            }

            return null;
        }

        public void RemovePart(string name)
        {
            var partDefinition = _contentDefinitionManager.LoadPartDefinition(name);

            if (partDefinition == null)
            {
                // Couldn't find this named part, ignore it.
                return;
            }

            var fieldDefinitions = partDefinition.Fields.ToArray();
            foreach (var fieldDefinition in fieldDefinitions)
            {
                RemoveFieldFromPart(fieldDefinition.Name, name);
            }

            _contentDefinitionManager.DeletePartDefinition(name);
            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartRemoved(context), new ContentPartRemovedContext { ContentPartDefinition = partDefinition }, _logger);
        }

        public IEnumerable<Type> GetFields()
        {
            return _contentFieldTypes;
        }

        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName)
        {
            AddFieldToPart(fieldName, fieldName, fieldTypeName, partName);
        }

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName)
        {
            if (String.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("The 'fieldName' can't be null or empty.", nameof(fieldName));
            }

            var partDefinition = _contentDefinitionManager.LoadPartDefinition(partName);
            var typeDefinition = _contentDefinitionManager.LoadTypeDefinition(partName);

            // If the type exists ensure it has its own part.
            if (typeDefinition != null)
            {
                _contentDefinitionManager.AlterTypeDefinition(partName, builder => builder.WithPart(partName));
            }

            fieldName = fieldName.ToSafeName();

            _contentDefinitionManager.AlterPartDefinition(partName,
                partBuilder => partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName).WithDisplayName(displayName)));

            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentFieldAttached(context), new ContentFieldAttachedContext
            {
                ContentPartName = partName,
                ContentFieldTypeName = fieldTypeName,
                ContentFieldName = fieldName,
                ContentFieldDisplayName = displayName
            }, _logger);
        }

        public void RemoveFieldFromPart(string fieldName, string partName)
        {
            _contentDefinitionManager.AlterPartDefinition(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
            _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentFieldDetached(context), new ContentFieldDetachedContext
            {
                ContentPartName = partName,
                ContentFieldName = fieldName
            }, _logger);
        }

        public void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
        {
            _contentDefinitionManager.AlterPartDefinition(partViewModel.Name, partBuilder =>
            {
                partBuilder.WithField(fieldViewModel.Name, fieldBuilder =>
                {
                    fieldBuilder.WithDisplayName(fieldViewModel.DisplayName);
                    fieldBuilder.WithEditor(fieldViewModel.Editor);
                    fieldBuilder.WithDisplayMode(fieldViewModel.DisplayMode);
                });
            });
        }

        public void AlterTypePart(EditTypePartViewModel typePartViewModel)
        {
            var typeDefinition = typePartViewModel.TypePartDefinition.ContentTypeDefinition;

            _contentDefinitionManager.AlterTypeDefinition(typeDefinition.Name, type =>
            {
                type.WithPart(typePartViewModel.Name, typePartViewModel.TypePartDefinition.PartDefinition, part =>
                {
                    part.WithDisplayName(typePartViewModel.DisplayName);
                    part.WithDescription(typePartViewModel.Description);
                    part.WithEditor(typePartViewModel.Editor);
                    part.WithDisplayMode(typePartViewModel.DisplayMode);
                });
            });
        }

        public void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames)
        {
            _contentDefinitionManager.AlterTypeDefinition(typeDefinition.Name, type =>
            {
                for (var i = 0; i < partNames.Length; i++)
                {
                    var partDefinition = typeDefinition.Parts.FirstOrDefault(x => x.Name == partNames[i]);
                    type.WithPart(partNames[i], partDefinition.PartDefinition, part =>
                    {
                        part.MergeSettings<ContentTypePartSettings>(x => x.Position = i.ToString());
                    });
                }
            });
        }

        public void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames)
        {
            _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, type =>
            {
                for (var i = 0; i < fieldNames.Length; i++)
                {
                    var fieldDefinition = partDefinition.Fields.FirstOrDefault(x => x.Name == fieldNames[i]);
                    type.WithField(fieldNames[i], field =>
                    {
                        field.MergeSettings<ContentPartFieldSettings>(x => x.Position = i.ToString());
                    });
                }
            });
        }

        public string GenerateContentTypeNameFromDisplayName(string displayName)
        {
            displayName = displayName.ToSafeName();

            while (_contentDefinitionManager.LoadTypeDefinition(displayName) != null)
                displayName = VersionName(displayName);

            return displayName;
        }

        public string GenerateFieldNameFromDisplayName(string partName, string displayName)
        {
            IEnumerable<ContentPartFieldDefinition> fieldDefinitions;

            var part = _contentDefinitionManager.LoadPartDefinition(partName);
            displayName = displayName.ToSafeName();

            if (part == null)
            {
                var type = _contentDefinitionManager.LoadTypeDefinition(partName)
                    ?? throw new ArgumentException("The part doesn't exist: " + partName);

                var typePart = type.Parts.FirstOrDefault(x => x.PartDefinition.Name == partName);

                // Id passed in might be that of a type w/ no implicit field.
                if (typePart == null)
                {
                    return displayName;
                }
                else
                {
                    fieldDefinitions = typePart.PartDefinition.Fields.ToArray();
                }
            }
            else
            {
                fieldDefinitions = part.Fields.ToArray();
            }

            while (fieldDefinitions.Any(x => String.Equals(displayName.Trim(), x.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                displayName = VersionName(displayName);

            return displayName;
        }

        private static string VersionName(string name)
        {
            int version;
            var nameParts = name.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1 && Int32.TryParse(nameParts.Last(), out version))
            {
                version = version > 0 ? ++version : 2;

                // This could unintentionally chomp something that looks like a version.
                name = String.Join("-", nameParts.Take(nameParts.Length - 1));
            }
            else
            {
                version = 2;
            }

            return String.Format("{0}-{1}", name, version);
        }
    }
}
