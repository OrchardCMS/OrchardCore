using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;
using YesSql;

namespace OrchardCore.ContentTypes.Services
{
    public class ContentDefinitionService : IContentDefinitionService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentDefinitionEventHandler> _contentDefinitionEventHandlers;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IEnumerable<ContentPart> _contentParts;
        private readonly IEnumerable<ContentField> _contentFields;

        public ContentDefinitionService(
                IContentDefinitionManager contentDefinitionManager,
                IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers,
                IContentManager contentManager,
                ISession session,
                IEnumerable<ContentPart> contentParts,
                IEnumerable<ContentField> contentFields,
                ILogger<IContentDefinitionService> logger,
                IStringLocalizer<ContentDefinitionService> localizer)
        {
            _session = session;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentDefinitionEventHandlers = contentDefinitionEventHandlers;
            _contentParts = contentParts;
            _contentFields = contentFields;

            Logger = logger;
            T = localizer;
        }

        public ILogger Logger { get; }
        public IStringLocalizer T { get; set; }

        public async Task<IEnumerable<EditTypeViewModel>> GetTypesAsync()
        {
            return (await _contentDefinitionManager
                .ListTypeDefinitionsAsync())
                .Select(ctd => new EditTypeViewModel(ctd))
                .OrderBy(m => m.DisplayName);
        }

        public async Task<EditTypeViewModel> GetTypeAsync(string name)
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(name);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            return new EditTypeViewModel(contentTypeDefinition);
        }

        public async Task<ContentTypeDefinition> AddTypeAsync(string name, string displayName)
        {
            if (String.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(nameof(displayName));
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                name = await GenerateContentTypeNameFromDisplayNameAsync(displayName);
            }
            else
            {
                if (!name[0].IsLetter())
                {
                    throw new ArgumentException("Content type name must start with a letter", "name");
                }
            }

            while (await _contentDefinitionManager.GetTypeDefinitionAsync(name) != null)
            {
                name = VersionName(name);
            }

            var contentTypeDefinition = new ContentTypeDefinition(name, displayName);


            await _contentDefinitionManager.StoreTypeDefinitionAsync(contentTypeDefinition);
            // Ensure it has its own part
            await _contentDefinitionManager.AlterTypeDefinitionAsync(name, builder => builder.WithPart(name));
            await _contentDefinitionManager.AlterTypeDefinitionAsync(name, cfg => cfg.Creatable().Draftable().Versionable().Listable().Securable());

            _contentDefinitionEventHandlers.Invoke(x => x.ContentTypeCreated(new ContentTypeCreatedContext { ContentTypeDefinition = contentTypeDefinition }), Logger);

            return contentTypeDefinition;
        }

        public async Task RemoveTypeAsync(string name, bool deleteContent)
        {

            // first remove all attached parts
            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(name);
            var partDefinitions = typeDefinition.Parts.ToArray();
            foreach (var partDefinition in partDefinitions)
            {
                await RemovePartFromTypeAsync(partDefinition.PartDefinition.Name, name);

                // delete the part if it's its own part
                if (partDefinition.PartDefinition.Name == name)
                {
                    await RemovePartAsync(name);
                }
            }

            await _contentDefinitionManager.DeleteTypeDefinitionAsync(name);

            _contentDefinitionEventHandlers.Invoke(x => x.ContentTypeRemoved(new ContentTypeRemovedContext { ContentTypeDefinition = typeDefinition }), Logger);
        }

        public async Task AddPartToTypeAsync(string partName, string typeName)
        {
            await _contentDefinitionManager.AlterTypeDefinitionAsync(typeName, typeBuilder => typeBuilder.WithPart(partName));
            _contentDefinitionEventHandlers.Invoke(x => x.ContentPartAttached(new ContentPartAttachedContext { ContentTypeName = typeName, ContentPartName = partName }), Logger);
        }

        public async Task AddReusablePartToTypeAsync(string name, string displayName, string description, string partName, string typeName)
        {
            await _contentDefinitionManager.AlterTypeDefinitionAsync(typeName, typeBuilder => typeBuilder.WithPart(name, partName, cfg =>
            {
                cfg.WithDisplayName(displayName);
                cfg.WithDescription(description);
            }));

            _contentDefinitionEventHandlers.Invoke(x => x.ContentPartAttached(new ContentPartAttachedContext { ContentTypeName = typeName, ContentPartName = partName }), Logger);
        }

        public async Task RemovePartFromTypeAsync(string partName, string typeName)
        {
            await _contentDefinitionManager.AlterTypeDefinitionAsync(typeName, typeBuilder => typeBuilder.RemovePart(partName));
            _contentDefinitionEventHandlers.Invoke(x => x.ContentPartDetached(new ContentPartDetachedContext { ContentTypeName = typeName, ContentPartName = partName }), Logger);
        }

        public async Task<IEnumerable<EditPartViewModel>> GetPartsAsync(bool metadataPartsOnly)
        {
            var typeNames = new HashSet<string>((await GetTypesAsync()).Select(ctd => ctd.Name));

            // user-defined parts
            // except for those parts with the same name as a type (implicit type's part or a mistake)
            var userContentParts = (await _contentDefinitionManager.ListPartDefinitionsAsync())
                .Where(cpd => !typeNames.Contains(cpd.Name))
                .Select(cpd => new EditPartViewModel(cpd))
                .ToDictionary(
                    k => k.Name,
                    v => v);

            // code-defined parts
            var codeDefinedParts = metadataPartsOnly
                ? Enumerable.Empty<EditPartViewModel>()
                : _contentParts
                        .Where(cpd => !userContentParts.ContainsKey(cpd.GetType().Name))
                        .Select(cpi => new EditPartViewModel { Name = cpi.GetType().Name, DisplayName = cpi.GetType().Name })
                    .ToList();

            // Order by display name
            return codeDefinedParts
                .Union(userContentParts.Values)
                .OrderBy(m => m.DisplayName);
        }

        public async Task<EditPartViewModel> GetPartAsync(string name)
        {
            var contentPartDefinition = await _contentDefinitionManager.GetPartDefinitionAsync(name);

            if (contentPartDefinition == null)
            {
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(name);

                if (contentTypeDefinition == null)
                {
                    return null;
                }

                contentPartDefinition = new ContentPartDefinition(name);
            }

            var viewModel = new EditPartViewModel(contentPartDefinition);

            return viewModel;
        }

        public async Task<EditPartViewModel> AddPartAsync(CreatePartViewModel partViewModel)
        {
            var name = partViewModel.Name;

            if (await _contentDefinitionManager.GetPartDefinitionAsync(name) != null)
                throw new Exception(T["Cannot add part named '{0}'. It already exists.", name]);

            if (!string.IsNullOrEmpty(name))
            {
                await _contentDefinitionManager.AlterPartDefinitionAsync(name, builder => builder.Attachable());
                var partDefinition = await _contentDefinitionManager.GetPartDefinitionAsync(name);
                _contentDefinitionEventHandlers.Invoke(x => x.ContentPartCreated(new ContentPartCreatedContext { ContentPartDefinition = partDefinition }), Logger);
                return new EditPartViewModel(partDefinition);
            }

            return null;
        }

        public async Task RemovePartAsync(string name)
        {
            var partDefinition = await _contentDefinitionManager.GetPartDefinitionAsync(name);

            if (partDefinition == null)
            {
                // Couldn't find this named part, ignore it
                return;
            }

            var fieldDefinitions = partDefinition.Fields.ToArray();
            foreach (var fieldDefinition in fieldDefinitions)
            {
                await RemoveFieldFromPartAsync(fieldDefinition.Name, name);
            }

            await _contentDefinitionManager.DeletePartDefinitionAsync(name);
            _contentDefinitionEventHandlers.Invoke(x => x.ContentPartRemoved(new ContentPartRemovedContext { ContentPartDefinition = partDefinition }), Logger);
        }

        public Task<IEnumerable<Type>> GetFieldsAsync()
        {
            return Task.FromResult((IEnumerable<Type>)_contentFields.Select(x => x.GetType()).ToArray());
        }

        public Task AddFieldToPartAsync(string fieldName, string fieldTypeName, string partName)
        {
            return AddFieldToPartAsync(fieldName, fieldName, fieldTypeName, partName);
        }

        public async Task AddFieldToPartAsync(string fieldName, string displayName, string fieldTypeName, string partName)
        {
            if (String.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException(nameof(fieldName));
            }

            var partDefinition = await _contentDefinitionManager.GetPartDefinitionAsync(partName);
            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(partName);

            // If the type exists ensure it has its own part
            if (typeDefinition != null)
            {
                await _contentDefinitionManager.AlterTypeDefinitionAsync(partName, builder => builder.WithPart(partName));
            }

            fieldName = fieldName.ToSafeName();

            await _contentDefinitionManager.AlterPartDefinitionAsync(partName,
                partBuilder => partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName).WithDisplayName(displayName)));

            _contentDefinitionEventHandlers.Invoke(x => x.ContentFieldAttached(new ContentFieldAttachedContext
            {
                ContentPartName = partName,
                ContentFieldTypeName = fieldTypeName,
                ContentFieldName = fieldName,
                ContentFieldDisplayName = displayName
            }), Logger);
        }

        public async Task RemoveFieldFromPartAsync(string fieldName, string partName)
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync(partName, typeBuilder => typeBuilder.RemoveField(fieldName));
            _contentDefinitionEventHandlers.Invoke(x => x.ContentFieldDetached(new ContentFieldDetachedContext
            {
                ContentPartName = partName,
                ContentFieldName = fieldName
            }), Logger);
        }

        public Task AlterFieldAsync(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
        {
            return _contentDefinitionManager.AlterPartDefinitionAsync(partViewModel.Name, partBuilder =>
            {
                partBuilder.WithField(fieldViewModel.Name, fieldBuilder =>
                {
                    fieldBuilder.WithDisplayName(fieldViewModel.DisplayName);
                    fieldBuilder.WithEditor(fieldViewModel.Editor);
                    fieldBuilder.WithDisplayMode(fieldViewModel.DisplayMode);
                });
            });
        }

        public Task AlterTypePartAsync(EditTypePartViewModel typePartViewModel)
        {
            var typeDefinition = typePartViewModel.TypePartDefinition.ContentTypeDefinition;

            return _contentDefinitionManager.AlterTypeDefinitionAsync(typeDefinition.Name, type =>
            {
                type.WithPart(typePartViewModel.Name, typePartViewModel.TypePartDefinition.PartDefinition, part =>
                {
                    part.WithDisplayName(typePartViewModel.DisplayName);
                    part.WithDescription(typePartViewModel.Description);
                    part.WithEditor(typePartViewModel.Editor);
                });
            });
        }

        public Task AlterTypePartsOrderAsync(ContentTypeDefinition typeDefinition, string[] partNames)
        {
            return _contentDefinitionManager.AlterTypeDefinitionAsync(typeDefinition.Name, type =>
            {
                for (var i = 0; i < partNames.Length; i++)
                {
                    var partDefinition = typeDefinition.Parts.FirstOrDefault(x => x.Name == partNames[i]);
                    type.WithPart(partNames[i], partDefinition.PartDefinition, part =>
                    {
                        part.WithSetting("Position", i.ToString());
                    });
                }
            });
        }

        public Task AlterPartFieldsOrderAsync(ContentPartDefinition partDefinition, string[] fieldNames)
        {
            return _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, type =>
            {
                for (var i = 0; i < fieldNames.Length; i++)
                {
                    var fieldDefinition = partDefinition.Fields.FirstOrDefault(x => x.Name == fieldNames[i]);
                    type.WithField(fieldNames[i], field =>
                    {
                        field.WithSetting("Position", i.ToString());
                    });
                }
            });
        }

        public async Task<string> GenerateContentTypeNameFromDisplayNameAsync(string displayName)
        {
            displayName = displayName.ToSafeName();

            while (await _contentDefinitionManager.GetTypeDefinitionAsync(displayName) != null)
            {
                displayName = VersionName(displayName);
            }

            return displayName;
        }

        public async Task<string> GenerateFieldNameFromDisplayNameAsync(string partName, string displayName)
        {
            IEnumerable<ContentPartFieldDefinition> fieldDefinitions;

            var part = await _contentDefinitionManager.GetPartDefinitionAsync(partName);
            displayName = displayName.ToSafeName();

            if (part == null)
            {
                var type = await _contentDefinitionManager.GetTypeDefinitionAsync(partName);

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
            var nameParts = name.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length > 1 && int.TryParse(nameParts.Last(), out version))
            {
                version = version > 0 ? ++version : 2;
                //this could unintentionally chomp something that looks like a version
                name = string.Join("-", nameParts.Take(nameParts.Length - 1));
            }
            else
            {
                version = 2;
            }

            return string.Format("{0}-{1}", name, version);
        }
    }
}
