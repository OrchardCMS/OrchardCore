using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Services;
using OrchardCore.Media.Settings;
using OrchardCore.Media.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Media.Drivers
{
    public class MediaFieldDisplayDriver : ContentFieldDisplayDriver<MediaField>
    {
        private static readonly JsonSerializerOptions _settings = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly AttachedMediaFieldFileService _attachedMediaFieldFileService;
        protected readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public MediaFieldDisplayDriver(AttachedMediaFieldFileService attachedMediaFieldFileService,
            IStringLocalizer<MediaFieldDisplayDriver> localizer,
            ILogger<MediaFieldDisplayDriver> logger)
        {
            _attachedMediaFieldFileService = attachedMediaFieldFileService;
            S = localizer;
            _logger = logger;
        }

        public override IDisplayResult Display(MediaField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayMediaFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(MediaField field, BuildFieldEditorContext context)
        {
            var itemPaths = field.Paths?.ToList().Select(p => new EditMediaFieldItemInfo { Path = p }).ToArray() ?? Array.Empty<EditMediaFieldItemInfo>();

            return Initialize<EditMediaFieldViewModel>(GetEditorShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.GetSettings<MediaFieldSettings>();

                for (var i = 0; i < itemPaths.Length; i++)
                {
                    if (settings.AllowMediaText && i < field.MediaTexts?.Length)
                    {
                        itemPaths[i].MediaText = field.MediaTexts[i];
                    }

                    if (settings.AllowAnchors)
                    {
                        var anchors = field.GetAnchors();
                        if (anchors != null && i < anchors.Length)
                        {
                            itemPaths[i].Anchor = anchors[i];
                        }
                    }

                    var filenames = field.GetAttachedFileNames();
                    if (filenames != null && i < filenames.Length)
                    {
                        itemPaths[i].AttachedFileName = filenames[i];
                    }
                }

                model.Paths = JsonSerializer.Serialize(itemPaths, _settings);
                model.TempUploadFolder = _attachedMediaFieldFileService.MediaFieldsTempSubFolder;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
                model.AllowMediaText = settings.AllowMediaText;
                model.AllowedExtensions = settings.AllowedExtensions ?? Array.Empty<string>();
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MediaField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var model = new EditMediaFieldViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix, f => f.Paths))
            {
                // Deserializing an empty string doesn't return an array
                var items = String.IsNullOrWhiteSpace(model.Paths)
                    ? new List<EditMediaFieldItemInfo>()
                    : JsonSerializer.Deserialize<List<EditMediaFieldItemInfo>>(model.Paths, _settings);

                // If it's an attached media field editor the files are automatically handled by _attachedMediaFieldFileService.
                if (String.Equals(context.PartFieldDefinition.Editor(), "Attached", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        field.SetAttachedFileNames(items.Where(i => !i.IsRemoved).Select(i => i.AttachedFileName).ToArray());
                        await _attachedMediaFieldFileService.HandleFilesOnFieldUpdateAsync(items, context.ContentPart.ContentItem);
                    }
                    catch (Exception e)
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(model.Paths), S["{0}: There was an error handling the files.", context.PartFieldDefinition.DisplayName()]);
                        _logger.LogError(e, "Error handling attached media files for field '{Field}'", context.PartFieldDefinition.DisplayName());
                    }
                }

                field.Paths = items.Where(p => !p.IsRemoved).Select(p => p.Path).ToArray() ?? Array.Empty<string>();

                var settings = context.PartFieldDefinition.GetSettings<MediaFieldSettings>();

                if (settings.AllowedExtensions?.Length > 0)
                {
                    for (var i = 0; i < field.Paths.Length; i++)
                    {
                        var extension = Path.GetExtension(field.Paths[i]);

                        if (!settings.AllowedExtensions.Contains(extension))
                        {
                            updater.ModelState.AddModelError(Prefix, nameof(model.Paths), S["Media extension is not allowed. Only media with '{0}' extensions are allowed.", String.Join(", ", settings.AllowedExtensions)]);
                        }
                    }
                }

                if (settings.Required && field.Paths.Length < 1)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Paths), S["{0}: A media is required.", context.PartFieldDefinition.DisplayName()]);
                }

                if (field.Paths.Length > 1 && !settings.Multiple)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Paths), S["{0}: Selecting multiple media is forbidden.", context.PartFieldDefinition.DisplayName()]);
                }

                if (settings.AllowMediaText)
                {
                    field.MediaTexts = items.Select(t => t.MediaText).ToArray();
                }
                else
                {
                    field.MediaTexts = Array.Empty<string>();
                }

                if (settings.AllowAnchors)
                {
                    field.SetAnchors(items.Select(t => t.Anchor).ToArray());
                }
                else if (field.Content.ContainsKey("Anchors")) // Less well known properties should be self healing.
                {
                    field.Content.Remove("Anchors");
                }
            }

            return Edit(field, context);
        }
    }
}
