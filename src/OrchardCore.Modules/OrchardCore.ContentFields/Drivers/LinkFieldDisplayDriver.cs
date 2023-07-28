using System;
using System.Text.Encodings.Web;
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
using OrchardCore.Infrastructure.Html;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers
{
    public class LinkFieldDisplayDriver : ContentFieldDisplayDriver<LinkField>
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        protected readonly IStringLocalizer S;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly HtmlEncoder _htmlencoder;

        public LinkFieldDisplayDriver(
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IStringLocalizer<LinkFieldDisplayDriver> localizer,
            IHtmlSanitizerService htmlSanitizerService,
            HtmlEncoder htmlencoder)
        {
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            S = localizer;
            _htmlSanitizerService = htmlSanitizerService;
            _htmlencoder = htmlencoder;
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
                model.Url = context.IsNew && field.Url == null ? settings.DefaultUrl : field.Url;
                model.Text = context.IsNew && field.Text == null ? settings.DefaultText : field.Text;

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
                        urlToValidate = urlToValidate[..indexAnchor];
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
                else if (!String.IsNullOrWhiteSpace(field.Url))
                {
                    if (!Uri.IsWellFormedUriString(urlToValidate, UriKind.RelativeOrAbsolute))
                    {
                        updater.ModelState.AddModelError(Prefix, nameof(field.Url), S["{0} is an invalid url.", field.Url]);
                    }
                    else
                    {
                        var link = $"<a href=\"{_htmlencoder.Encode(urlToValidate)}\"></a>";

                        if (!String.Equals(link, _htmlSanitizerService.Sanitize(link), StringComparison.OrdinalIgnoreCase))
                        {
                            updater.ModelState.AddModelError(Prefix, nameof(field.Url), S["{0} is an invalid url.", field.Url]);
                        }
                    }
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
            }

            return Edit(field, context);
        }
    }
}
