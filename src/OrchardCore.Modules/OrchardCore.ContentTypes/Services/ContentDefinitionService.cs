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

namespace OrchardCore.ContentTypes.Services;

public class ContentDefinitionService : IContentDefinitionService
{
    private readonly IEnumerable<Type> _contentPartTypes;
    private readonly IEnumerable<Type> _contentFieldTypes;

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IEnumerable<IContentDefinitionEventHandler> _contentDefinitionEventHandlers;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;

    public ContentDefinitionService(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers,
            IEnumerable<ContentPart> contentParts,
            IEnumerable<ContentField> contentFields,
            IOptions<ContentOptions> contentOptions,
            ILogger<IContentDefinitionService> logger,
            IStringLocalizer<ContentDefinitionService> stringLocalizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentDefinitionEventHandlers = contentDefinitionEventHandlers;

        foreach (var element in contentParts.Select(x => x.GetType()))
        {
            logger.LogWarning("The content part '{ContentPart}' should not be registered in DI. Use AddContentPart<T> instead.", element);
        }

        foreach (var element in contentFields.Select(x => x.GetType()))
        {
            logger.LogWarning("The content field '{ContentField}' should not be registered in DI. Use AddContentField<T> instead.", element);
        }

        // TODO: This code can be removed in a future release and rationalized to only use ContentPartOptions.
        _contentPartTypes = contentParts.Select(cp => cp.GetType())
            .Union(contentOptions.Value.ContentPartOptions.Select(cpo => cpo.Type));

        // TODO: This code can be removed in a future release and rationalized to only use ContentFieldOptions.
        _contentFieldTypes = contentFields.Select(cf => cf.GetType())
            .Union(contentOptions.Value.ContentFieldOptions.Select(cfo => cfo.Type));

        _logger = logger;
        S = stringLocalizer;
    }

    public async Task<IEnumerable<EditTypeViewModel>> LoadTypesAsync()
        => (await _contentDefinitionManager.LoadTypeDefinitionsAsync())
            .Select(ctd => new EditTypeViewModel(ctd))
            .OrderBy(m => m.DisplayName);

    public async Task<IEnumerable<EditTypeViewModel>> GetTypesAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(ctd => new EditTypeViewModel(ctd))
            .OrderBy(m => m.DisplayName);

    public async Task<EditTypeViewModel> LoadTypeAsync(string name)
    {
        var contentTypeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);

        if (contentTypeDefinition == null)
        {
            return null;
        }

        return new EditTypeViewModel(contentTypeDefinition);
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
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException($"The '{nameof(displayName)}' can't be null or empty.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            name = await GenerateContentTypeNameFromDisplayNameAsync(displayName);
        }
        else
        {
            if (!name[0].IsLetter())
            {
                throw new ArgumentException("Content type name must start with a letter", nameof(name));
            }
            if (!string.Equals(name, name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Content type name contains invalid characters", nameof(name));
            }
        }

        while (await _contentDefinitionManager.LoadTypeDefinitionAsync(name) is not null)
        {
            name = VersionName(name);
        }

        var contentTypeDefinition = new ContentTypeDefinition(name, displayName);

        await _contentDefinitionManager.StoreTypeDefinitionAsync(contentTypeDefinition);

        // Ensure it has its own part.
        await _contentDefinitionManager.AlterTypeDefinitionAsync(name, builder => builder.WithPart(name));
        await _contentDefinitionManager.AlterTypeDefinitionAsync(name, cfg => cfg.Creatable().Draftable().Versionable().Listable().Securable());

        var context = new ContentTypeCreatedContext
        {
            ContentTypeDefinition = contentTypeDefinition,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentTypeCreated(ctx), context, _logger);

        return contentTypeDefinition;
    }

    public async Task RemoveTypeAsync(string name, bool deleteContent)
    {
        // First remove all attached parts.
        var typeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);

        if (typeDefinition == null)
        {
            return;
        }

        var settings = typeDefinition.GetSettings<ContentSettings>();

        if (settings.IsSystemDefined)
        {
            throw new InvalidOperationException("Unable to remove system-defined type.");
        }

        var partDefinitions = typeDefinition.Parts.ToList();
        foreach (var partDefinition in partDefinitions)
        {
            await RemovePartFromTypeAsync(partDefinition.PartDefinition.Name, name);

            // Delete the part if it's its own part.
            if (partDefinition.PartDefinition.Name == name)
            {
                await RemovePartAsync(name);
            }
        }

        await _contentDefinitionManager.DeleteTypeDefinitionAsync(name);

        var context = new ContentTypeRemovedContext
        {
            ContentTypeDefinition = typeDefinition,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentTypeRemoved(ctx), context, _logger);
    }

    public async Task AddPartToTypeAsync(string partName, string typeName)
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(typeName, typeBuilder => typeBuilder.WithPart(partName));
        var context = new ContentPartAttachedContext
        {
            ContentTypeName = typeName,
            ContentPartName = partName,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartAttached(ctx), context, _logger);
    }

    public async Task AddReusablePartToTypeAsync(string name, string displayName, string description, string partName, string typeName)
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(typeName, typeBuilder => typeBuilder
            .WithPart(name, partName, partBuilder =>
            {
                partBuilder.WithDisplayName(displayName);
                partBuilder.WithDescription(description);
            })
        );

        var context = new ContentPartAttachedContext
        {
            ContentTypeName = typeName,
            ContentPartName = partName,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartAttached(ctx), context, _logger);
    }

    public async Task RemovePartFromTypeAsync(string partName, string typeName)
    {
        var typeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(typeName);

        if (typeDefinition == null)
        {
            return;
        }

        var partDefinition = typeDefinition.Parts.FirstOrDefault(p => string.Equals(p.Name, partName, StringComparison.OrdinalIgnoreCase));

        if (partDefinition == null)
        {
            return;
        }

        var settings = partDefinition.GetSettings<ContentSettings>();

        if (settings.IsSystemDefined)
        {
            throw new InvalidOperationException("Unable to remove system-defined part.");
        }

        await _contentDefinitionManager.AlterTypeDefinitionAsync(typeName, typeBuilder => typeBuilder.RemovePart(partName));

        var context = new ContentPartDetachedContext
        {
            ContentTypeName = typeName,
            ContentPartName = partName,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartDetached(ctx), context, _logger);
    }

    public async Task<IEnumerable<EditPartViewModel>> LoadPartsAsync(bool metadataPartsOnly)
    {
        var typeNames = new HashSet<string>((await LoadTypesAsync()).Select(ctd => ctd.Name));

        // User-defined parts.
        // Except for those parts with the same name as a type (implicit type's part or a mistake).
        var userContentParts = (await _contentDefinitionManager.LoadPartDefinitionsAsync())
            .Where(cpd => !typeNames.Contains(cpd.Name))
            .Select(cpd => new EditPartViewModel(cpd))
            .ToDictionary(k => k.Name);

        // Code-defined parts.
        var codeDefinedParts = metadataPartsOnly
            ? []
            : _contentPartTypes
                .Where(cpd => !userContentParts.ContainsKey(cpd.Name))
                .Select(cpi => new EditPartViewModel
                {
                    Name = cpi.Name,
                    DisplayName = cpi.Name,
                }).ToList();

        // Order by display name.
        return codeDefinedParts
            .Union(userContentParts.Values)
            .OrderBy(m => m.DisplayName);
    }

    public async Task<IEnumerable<EditPartViewModel>> GetPartsAsync(bool metadataPartsOnly)
    {
        var typeNames = new HashSet<string>((await GetTypesAsync()).Select(ctd => ctd.Name));

        // User-defined parts.
        // Except for those parts with the same name as a type (implicit type's part or a mistake).
        var userContentParts = (await _contentDefinitionManager.ListPartDefinitionsAsync())
            .Where(cpd => !typeNames.Contains(cpd.Name))
            .Select(cpd => new EditPartViewModel(cpd))
            .ToDictionary(k => k.Name);

        // Code-defined parts.
        var codeDefinedParts = metadataPartsOnly
            ? []
            : _contentPartTypes
                .Where(cpd => !userContentParts.ContainsKey(cpd.Name))
                .Select(cpi => new EditPartViewModel
                {
                    Name = cpi.Name,
                    DisplayName = cpi.Name,
                }).ToList();

        // Order by display name.
        return codeDefinedParts
            .Union(userContentParts.Values)
            .OrderBy(m => m.DisplayName);
    }

    public async Task<EditPartViewModel> LoadPartAsync(string name)
    {
        var contentPartDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(name);

        if (contentPartDefinition == null)
        {
            var contentTypeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(name);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            contentPartDefinition = new ContentPartDefinition(name);
        }

        var viewModel = new EditPartViewModel(contentPartDefinition);

        return viewModel;
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

        if (await _contentDefinitionManager.LoadPartDefinitionAsync(name) is not null)
        {
            throw new Exception(S["Cannot add part named '{0}'. It already exists.", name]);
        }

        if (!string.IsNullOrEmpty(name))
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync(name, builder => builder.Attachable());
            var partDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(name);

            var context = new ContentPartCreatedContext
            {
                ContentPartDefinition = partDefinition,
            };

            _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartCreated(ctx), context, _logger);

            return new EditPartViewModel(partDefinition);
        }

        return null;
    }

    public async Task RemovePartAsync(string name)
    {
        var partDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(name);

        if (partDefinition == null)
        {
            // Couldn't find this named part, ignore it.
            return;
        }

        var settings = partDefinition.GetSettings<ContentSettings>();

        if (settings.IsSystemDefined)
        {
            throw new InvalidOperationException("Unable to remove system-defined part.");
        }

        foreach (var fieldDefinition in partDefinition.Fields)
        {
            await RemoveFieldFromPartAsync(fieldDefinition.Name, name);
        }

        await _contentDefinitionManager.DeletePartDefinitionAsync(name);

        var context = new ContentPartRemovedContext
        {
            ContentPartDefinition = partDefinition,
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartRemoved(ctx), context, _logger);
    }

    public Task<IEnumerable<Type>> GetFieldsAsync()
        => Task.FromResult(_contentFieldTypes);

    public Task AddFieldToPartAsync(string fieldName, string fieldTypeName, string partName)
        => AddFieldToPartAsync(fieldName, fieldName, fieldTypeName, partName);

    public async Task AddFieldToPartAsync(string fieldName, string displayName, string fieldTypeName, string partName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            throw new ArgumentException($"The '{nameof(fieldName)}' can't be null or empty.", nameof(fieldName));
        }

        var partDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(partName);
        var typeDefinition = await _contentDefinitionManager.LoadTypeDefinitionAsync(partName);

        // If the type exists ensure it has its own part.
        if (typeDefinition != null)
        {
            await _contentDefinitionManager.AlterTypeDefinitionAsync(partName, builder => builder.WithPart(partName));
        }

        fieldName = fieldName.ToSafeName();

        await _contentDefinitionManager.AlterPartDefinitionAsync(partName,
            partBuilder => partBuilder.WithField(fieldName, fieldBuilder => fieldBuilder.OfType(fieldTypeName).WithDisplayName(displayName)));

        _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentFieldAttached(context), new ContentFieldAttachedContext
        {
            ContentPartName = partName,
            ContentFieldTypeName = fieldTypeName,
            ContentFieldName = fieldName,
            ContentFieldDisplayName = displayName
        }, _logger);
    }

    public async Task RemoveFieldFromPartAsync(string fieldName, string partName)
    {
        var partDefinition = await _contentDefinitionManager.LoadPartDefinitionAsync(partName);

        if (partDefinition == null)
        {
            return;
        }

        var settings = partDefinition.GetSettings<ContentSettings>();

        if (settings.IsSystemDefined)
        {
            throw new InvalidOperationException("Unable to remove system-defined field.");
        }

        await _contentDefinitionManager.AlterPartDefinitionAsync(partName, typeBuilder => typeBuilder.RemoveField(fieldName));

        var context = new ContentFieldDetachedContext
        {
            ContentPartName = partName,
            ContentFieldName = fieldName
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentFieldDetached(ctx), context, _logger);
    }

    public async Task AlterFieldAsync(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel)
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(partViewModel.Name, partBuilder =>
        {
            partBuilder.WithField(fieldViewModel.Name, fieldBuilder =>
            {
                fieldBuilder.WithDisplayName(fieldViewModel.DisplayName);
                fieldBuilder.WithEditor(fieldViewModel.Editor);
                fieldBuilder.WithDisplayMode(fieldViewModel.DisplayMode);
            });
        });

        var context = new ContentPartFieldUpdatedContext
        {
            ContentPartName = partViewModel.Name,
            ContentFieldName = fieldViewModel.Name
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentPartFieldUpdated(ctx), context, _logger);
    }

    public async Task AlterTypePartAsync(EditTypePartViewModel typePartViewModel)
    {
        var typeDefinition = typePartViewModel.TypePartDefinition.ContentTypeDefinition;

        await _contentDefinitionManager.AlterTypeDefinitionAsync(typeDefinition.Name, type =>
        {
            type.WithPart(typePartViewModel.Name, typePartViewModel.TypePartDefinition.PartDefinition, part =>
            {
                part.WithDisplayName(typePartViewModel.DisplayName);
                part.WithDescription(typePartViewModel.Description);
                part.WithEditor(typePartViewModel.Editor);
                part.WithDisplayMode(typePartViewModel.DisplayMode);
            });
        });

        var context = new ContentTypePartUpdatedContext
        {
            ContentTypeName = typeDefinition.Name,
            ContentPartName = typePartViewModel.Name
        };

        _contentDefinitionEventHandlers.Invoke((handler, ctx) => handler.ContentTypePartUpdated(ctx), context, _logger);
    }

    public async Task AlterTypePartsOrderAsync(ContentTypeDefinition typeDefinition, string[] partNames)
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(typeDefinition.Name, type =>
        {
            if (partNames is null)
            {
                return;
            }

            for (var i = 0; i < partNames.Length; i++)
            {
                var partDefinition = typeDefinition.Parts?.FirstOrDefault(x => x.Name == partNames[i]);

                if (partDefinition == null)
                {
                    continue;
                }

                type.WithPart(partNames[i], partDefinition.PartDefinition, part =>
                {
                    part.MergeSettings<ContentTypePartSettings>(x => x.Position = i.ToString());
                });
            }
        });

        var context = new ContentTypeUpdatedContext
        {
            ContentTypeDefinition = typeDefinition
        };

        _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentTypeUpdated(context), context, _logger);
    }

    public async Task AlterPartFieldsOrderAsync(ContentPartDefinition partDefinition, string[] fieldNames)
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(partDefinition.Name, type =>
        {
            if (fieldNames is null)
            {
                return;
            }

            for (var i = 0; i < fieldNames.Length; i++)
            {
                var fieldDefinition = partDefinition.Fields.FirstOrDefault(x => x.Name == fieldNames[i]);
                type.WithField(fieldNames[i], field =>
                {
                    field.MergeSettings<ContentPartFieldSettings>(x => x.Position = i.ToString());
                });
            }
        });

        _contentDefinitionEventHandlers.Invoke((handler, context) => handler.ContentPartUpdated(context), new ContentPartUpdatedContext
        {
            ContentPartDefinition = partDefinition
        }, _logger);
    }

    public async Task<string> GenerateContentTypeNameFromDisplayNameAsync(string displayName)
    {
        displayName = displayName.ToSafeName();

        while (await _contentDefinitionManager.LoadTypeDefinitionAsync(displayName) != null)
        {
            displayName = VersionName(displayName);
        }

        return displayName;
    }

    public async Task<string> GenerateFieldNameFromDisplayNameAsync(string partName, string displayName)
    {
        IEnumerable<ContentPartFieldDefinition> fieldDefinitions;

        var part = await _contentDefinitionManager.LoadPartDefinitionAsync(partName);
        displayName = displayName.ToSafeName();

        if (part == null)
        {
            var type = await _contentDefinitionManager.LoadTypeDefinitionAsync(partName)
                ?? throw new ArgumentException("The part doesn't exist: " + partName);

            var typePart = type.Parts?.FirstOrDefault(x => x.PartDefinition.Name == partName);

            // If passed in might be that of a type w/ no implicit field.
            if (typePart == null)
            {
                return displayName;
            }

            fieldDefinitions = typePart.PartDefinition.Fields.ToList();
        }
        else
        {
            fieldDefinitions = part.Fields.ToList();
        }

        while (fieldDefinitions.Any(x => string.Equals(displayName.Trim(), x.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            displayName = VersionName(displayName);
        }

        return displayName;
    }

    private static string VersionName(string name)
    {
        int version;
        var nameParts = name.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length > 1 && int.TryParse(nameParts.Last(), out version))
        {
            version = version > 0 ? ++version : 2;

            // This could unintentionally chomp something that looks like a version.
            name = string.Join('-', nameParts.Take(nameParts.Length - 1));
        }
        else
        {
            version = 2;
        }

        return string.Format("{0}-{1}", name, version);
    }
}
