using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;
using System.Threading.Tasks;

namespace OrchardCore.Title.Drivers
{
    public class TitlePartDisplayDriver : ContentPartDisplayDriver<TitlePart>
    {
        private readonly IStringLocalizer S;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public TitlePartDisplayDriver(
            IStringLocalizer<TitlePartDisplayDriver> localizer,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            S = localizer;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override IDisplayResult Display(TitlePart titlePart, BuildPartDisplayContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();

            if (!settings.RenderTitle || string.IsNullOrWhiteSpace(titlePart.Title))
            {
                return null;
            }

            return Initialize<TitlePartViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Title = titlePart.ContentItem.DisplayText;
                model.TitlePart = titlePart;
                model.ContentItem = titlePart.ContentItem;
            })
            .Location("Detail", "Header:5")
            .Location("Summary", "Header:5");
        }

        public override IDisplayResult Edit(TitlePart titlePart, BuildPartEditorContext context)
        {
            return Initialize<TitlePartViewModel>(GetEditorShapeType(context), async model =>
            {
                model.Title = titlePart.ContentItem.DisplayText;
                model.TitlePart = titlePart;
                model.ContentItem = titlePart.ContentItem;
                model.Settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();
                model.IsEditable = await IsTitleEditableAsync(titlePart, context);
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TitlePart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            if (await IsTitleEditableAsync(model, context) && await updater.TryUpdateModelAsync(model, Prefix, t => t.Title))
            {
                var settings = context.TypePartDefinition.GetSettings<TitlePartSettings>();
                if (settings.Options == TitlePartOptions.EditableRequired && string.IsNullOrWhiteSpace(model.Title))
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

        private async Task<bool> IsTitleEditableAsync(TitlePart model, BuildPartEditorContext context)
        {
            return context.IsNew || await _authorizationService.AuthorizeAsync(
                _httpContextAccessor.HttpContext.User,
                Permissions.EditTitlePart,
                model.ContentItem);
        }
    }
}
