using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

public sealed class ContentTypeSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private static readonly ContentTypeDefinitionDriverOptions _defaultOptions = new();
    private readonly IStereotypeService _stereotypeService;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ContentTypeDefinitionOptions _options;

    internal readonly IStringLocalizer S;

    public ContentTypeSettingsDisplayDriver(
        IStringLocalizer<ContentTypeSettingsDisplayDriver> stringLocalizer,
        IOptions<ContentTypeDefinitionOptions> options,
        IStereotypeService stereotypeService,
        IContentDefinitionManager contentDefinitionManager)
    {
        S = stringLocalizer;
        _options = options.Value;
        _stereotypeService = stereotypeService;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<ContentTypeSettingsViewModel>("ContentTypeSettings_Edit", async model =>
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeSettings>();
            model.Creatable = settings.Creatable;
            model.Listable = settings.Listable;
            model.Draftable = settings.Draftable;
            model.Versionable = settings.Versionable;
            model.Securable = settings.Securable;
            model.Stereotype = settings.Stereotype;
            model.Description = settings.Description;
            model.Category = settings.Category;
            model.ThumbnailPath = settings.ThumbnailPath;
            model.Options = await GetOptionsAsync(contentTypeDefinition, settings.Stereotype);
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ContentTypeSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var stereotype = model.Stereotype?.Trim();
        context.Builder.WithDescription(model.Description);
        context.Builder.Stereotype(stereotype);
        context.Builder.WithCategory(model.Category);
        context.Builder.WithThumbnailPath(model.ThumbnailPath);

        if (!IsAlphaNumericOrEmpty(stereotype))
        {
            context.Updater.ModelState.AddModelError(nameof(ContentTypeSettingsViewModel.Stereotype), S["The stereotype should be alphanumeric."]);
        }

        var options = await GetOptionsAsync(contentTypeDefinition, stereotype);

        Apply(context, model, options);

        return Edit(contentTypeDefinition, context);
    }

    private static void Apply(UpdateTypeEditorContext context, ContentTypeSettingsViewModel model, ContentTypeDefinitionDriverOptions options)
    {
        if (options.ShowVersionable)
        {
            context.Builder.Versionable(model.Versionable);
        }

        if (options.ShowCreatable)
        {
            context.Builder.Creatable(model.Creatable);
        }

        if (options.ShowSecurable)
        {
            context.Builder.Securable(model.Securable);
        }

        if (options.ShowListable)
        {
            context.Builder.Listable(model.Listable);
        }

        if (options.ShowDraftable)
        {
            context.Builder.Draftable(model.Draftable);
        }
    }

    private async Task<ContentTypeDefinitionDriverOptions> GetOptionsAsync(ContentTypeDefinition contentTypeDefinition, string stereotype)
    {
        var options = _defaultOptions;

        if (contentTypeDefinition.Name != null
            && _options.ContentTypes.TryGetValue(contentTypeDefinition.Name, out var typeOptions))
        {
            options = typeOptions;
        }

        if (stereotype != null
            && _options.Stereotypes.TryGetValue(stereotype, out var stereotypesOptions))
        {
            options = stereotypesOptions;
        }

        options.Stereotypes = await _stereotypeService.GetStereotypesAsync();

        options.Categories = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(t => t.GetSettings<ContentTypeSettings>()?.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c);

        return options;
    }

    private static bool IsAlphaNumericOrEmpty(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var startWithLetter = char.IsLetter(value[0]);

        return value.Length == 1
            ? startWithLetter
            : startWithLetter && value.Skip(1).All(c => char.IsLetterOrDigit(c));
    }
}
