using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers
{
    public class LinkFieldDisplayDriver : ContentFieldDisplayDriver<LinkField>
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IStringLocalizer S;

        public LinkFieldDisplayDriver(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IStringLocalizer<LinkFieldDisplayDriver> localizer)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            S = localizer;
        }

        public override IDisplayResult Display(LinkField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayLinkFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(LinkField field, BuildFieldEditorContext context)
        {
            return Initialize<EditLinkFieldViewModel>(GetEditorShapeType(context), model =>
            {
                var settings = context.PartFieldDefinition.GetSettings<LinkFieldSettings>();
                model.Url = context.IsNew ? settings.DefaultUrl : field.Url;
                model.Text = context.IsNew ? settings.DefaultText : field.Text;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(LinkField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            var modelUpdated = await updater.TryUpdateModelAsync(field, Prefix, f => f.Url, f => f.Text);

            if (modelUpdated)
            {
                var settings = context.PartFieldDefinition.GetSettings<LinkFieldSettings>();

                var urlToValidate = field.Url;
                if (!String.IsNullOrEmpty(urlToValidate))
                {
                    var indexAnchor = urlToValidate.IndexOf('#');
                    if (indexAnchor > -1)
                    {
                        urlToValidate = urlToValidate.Substring(0, indexAnchor);
                    }

                    if (urlToValidate.StartsWith("~/", StringComparison.Ordinal))
                    {
                        var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                        urlToValidate = urlHelper.Content(urlToValidate);
                    }

                    urlToValidate = urlToValidate.ToUriComponents();
                }

                // Validate Url
                if (settings.Required && String.IsNullOrWhiteSpace(field.Url))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Url), S["The url is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
                else if (!String.IsNullOrWhiteSpace(field.Url) && !Uri.IsWellFormedUriString(urlToValidate, UriKind.RelativeOrAbsolute))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Url), S["{0} is an invalid url.", field.Url]);
                }

                // Validate Text
                if (settings.LinkTextMode == LinkTextMode.Required && String.IsNullOrWhiteSpace(field.Text))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Text), S["The link text is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
                else if (settings.LinkTextMode == LinkTextMode.Static && String.IsNullOrWhiteSpace(settings.DefaultText))
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Text), S["The text default value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }

                // Run this through a sanitizer in case someone puts html in it.
                // No settings.
            }

            return Edit(field, context);
        }
    }
}
