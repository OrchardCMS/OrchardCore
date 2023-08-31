using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title.Drivers
{
    public class TitlePartDisplayDriver : ContentPartDisplayDriver<TitlePart>
    {
        protected readonly IStringLocalizer S;

        public TitlePartDisplayDriver(IStringLocalizer<TitlePartDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(TitlePart titlePart, BuildPartDisplayContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();

            if (!settings.RenderTitle || String.IsNullOrWhiteSpace(titlePart.Title))
            {
                return null;
            }

            return Initialize<TitlePartViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Title = titlePart.ContentItem.DisplayText;
                model.TitlePart = titlePart;
                model.ContentItem = titlePart.ContentItem;
            })
            .Location("Detail", "Header")
            .Location("Summary", "Header");
        }

        public override IDisplayResult Edit(TitlePart titlePart, BuildPartEditorContext context)
        {
            return Initialize<TitlePartViewModel>(GetEditorShapeType(context), model =>
            {
                model.Title = titlePart.ContentItem.DisplayText;
                model.TitlePart = titlePart;
                model.ContentItem = titlePart.ContentItem;
                model.Settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TitlePart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(model, Prefix, t => t.Title))
            {
                var settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();
                if (settings.Options == TitlePartOptions.EditableRequired && String.IsNullOrWhiteSpace(model.Title))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(model.Title), S["A value is required for Title."]);
                }
                else
                {
                    model.ContentItem.DisplayText = model.Title;
                }
            }

            return Edit(model, context);
        }
    }
}
