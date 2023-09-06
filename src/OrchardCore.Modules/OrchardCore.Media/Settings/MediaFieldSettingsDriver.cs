using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.Fields;
using OrchardCore.Media.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Media.Settings
{
    public class MediaFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MediaField>
    {
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly MediaOptions _mediaOptions;
        protected readonly IStringLocalizer S;

        public MediaFieldSettingsDriver(
            IContentTypeProvider contentTypeProvider,
            IOptions<MediaOptions> mediaOptions,
            IStringLocalizer<MediaFieldSettingsDriver> stringLocalizer)
        {
            _contentTypeProvider = contentTypeProvider;
            _mediaOptions = mediaOptions.Value;
            S = stringLocalizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MediaFieldSettingsViewModel>("MediaFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<MediaFieldSettings>();

                model.Hint = settings.Hint;
                model.Required = settings.Required;
                model.Multiple = settings.Multiple;
                model.AllowMediaText = settings.AllowMediaText;
                model.AllowAnchors = settings.AllowAnchors;
                model.AllowAllDefaultMediaTypes = settings.AllowedExtensions?.Length == 0;

                var items = new List<MediaTypeViewModel>();
                foreach (var extension in _mediaOptions.AllowedFileExtensions)
                {
                    if (_contentTypeProvider.TryGetContentType(extension, out var contentType))
                    {
                        var item = new MediaTypeViewModel()
                        {
                            Extension = extension,
                            ContentType = contentType,
                            IsSelected = settings.AllowedExtensions != null && settings.AllowedExtensions.Contains(extension)
                        };

                        var index = contentType.IndexOf('/');

                        if (index > -1)
                        {
                            item.Type = contentType[..index];
                        }

                        items.Add(item);
                    }
                }
                model.MediaTypes = items
                .OrderBy(vm => vm.ContentType)
                .ToArray();

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new MediaFieldSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var settings = new MediaFieldSettings()
                {
                    Hint = model.Hint,
                    Required = model.Required,
                    Multiple = model.Multiple,
                    AllowMediaText = model.AllowMediaText,
                    AllowAnchors = model.AllowAnchors,
                };

                if (!model.AllowAllDefaultMediaTypes)
                {
                    var selectedExtensions = model.MediaTypes.Where(vm => vm.IsSelected && _mediaOptions.AllowedFileExtensions.Contains(vm.Extension))
                        .Select(x => x.Extension)
                        .ToArray();

                    if (selectedExtensions.Length == 0)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, String.Empty, S["Please select at least one extension."]);
                    }

                    settings.AllowedExtensions = selectedExtensions;
                }

                if (context.Updater.ModelState.IsValid)
                {
                    context.Builder.WithSettings(settings);
                }
            }

            return Edit(partFieldDefinition);
        }
    }
}
