using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.ViewModels;
using OrchardCore.ReCaptchaV3.Configuration;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Forms.Drivers
{
    public class ButtonPartDisplayDriver : ContentPartDisplayDriver<ButtonPart>
    {
        private readonly ISiteService _siteService;
        private readonly IResourceManager _resourceManager;

        public ButtonPartDisplayDriver(ISiteService siteService, IResourceManager resourceManager)
        {
            _siteService = siteService;
            _resourceManager = resourceManager;
        }
        public override IDisplayResult Display(ButtonPart part)
        {
            var recaptchaSettings = GetReCaptchaV3Settings();

            if (part.ReCaptchaV3Protected && recaptchaSettings.IsValid() && !string.IsNullOrWhiteSpace(part.FormId))
            {
                var builder = new TagBuilder("script");

                builder.InnerHtml.Append($"window.onSubmit = function onSubmit(token) {{ document.getElementById(\"{part.FormId}\").submit(); }}");
                _resourceManager.RegisterHeadScript(builder);

                builder = new TagBuilder("script");

                builder.Attributes.Add("src", recaptchaSettings.ReCaptchaV3ScriptUri);
                _resourceManager.RegisterFootScript(builder);

                part.SiteKey = recaptchaSettings.SiteKey;
            }

            return View("ButtonPart", part).Location("Detail", "Content");
        }

        public override IDisplayResult Edit(ButtonPart part)
        {
            return Initialize<ButtonPartEditViewModel>("ButtonPart_Fields_Edit", m =>
            {
                m.Text = part.Text;
                m.Type = part.Type;
                m.ReCaptchaSettingsAreConfigured = GetReCaptchaV3Settings().IsValid();
                m.ReCaptchaV3Protected = part.ReCaptchaV3Protected;
                m.FormId = part.FormId;
            });
        }

        public async override Task<IDisplayResult> UpdateAsync(ButtonPart part, IUpdateModel updater)
        {
            var viewModel = new ButtonPartEditViewModel();

            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                part.Text = viewModel.Text?.Trim();
                part.Type = viewModel.Type?.Trim();
                part.ReCaptchaV3Protected = viewModel.ReCaptchaV3Protected;
                part.FormId = viewModel.FormId?.Trim();
            }

            return Edit(part);
        }

        private ReCaptchaV3Settings GetReCaptchaV3Settings()
        {
            var siteSettings = _siteService.GetSiteSettingsAsync().Result;
            return siteSettings.As<ReCaptchaV3Settings>();
        }
    }
}
