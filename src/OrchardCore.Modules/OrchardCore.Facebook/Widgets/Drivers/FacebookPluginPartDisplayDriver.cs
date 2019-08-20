using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Mvc.ModelBinding;
using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Facebook.Widgets.Settings;

namespace OrchardCore.Facebook.Widgets.Drivers
{
    public class FacebookPluginPartDisplayDriver : ContentPartDisplayDriver<FacebookPluginPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer<FacebookPluginPartDisplayDriver> T;

        public FacebookPluginPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<FacebookPluginPartDisplayDriver> localizer
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            T = localizer;
        }

        public override IDisplayResult Display(FacebookPluginPart part)
        {
            return Combine(
                Initialize<FacebookPluginPartViewModel>("FacebookPluginPart", m => BuildViewModel(m, part))
                    .Location("Detail", "Content:10"),
                Initialize<FacebookPluginPartViewModel>("FacebookPluginPart_Summary", m => BuildViewModel(m, part))
                    .Location("Summary", "Content:10")
            );
        }

        private void BuildViewModel(FacebookPluginPartViewModel model, FacebookPluginPart part)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.FacebookPluginPart = part ?? throw new ArgumentNullException(nameof(part));
            model.Settings = GetFacebookPluginPartSettings(part);
            model.Liquid = part.Liquid;
            model.ContentItem = part.ContentItem;
        }

        public override IDisplayResult Edit(FacebookPluginPart part)
        {
            return Initialize<FacebookPluginPartViewModel>("FacebookPluginPart_Edit", model =>
            {
                model.Settings = GetFacebookPluginPartSettings(part);
                model.FacebookPluginPart = part;
                model.Liquid = string.IsNullOrWhiteSpace(part.Liquid) ? model.Settings.Liquid : part.Liquid;
            });
        }

        private FacebookPluginPartSettings GetFacebookPluginPartSettings(FacebookPluginPart part)
        {
            if (part == null)
            {
                throw new ArgumentNullException(nameof(part));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(FacebookPluginPart), StringComparison.Ordinal));
            return contentTypePartDefinition.GetSettings<FacebookPluginPartSettings>();
        }

        public override async Task<IDisplayResult> UpdateAsync(FacebookPluginPart model, IUpdateModel updater)
        {
            var viewModel = new FacebookPluginPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Liquid);

            model.Liquid = viewModel.Liquid;

            await ValidateAsync(model, updater);

            return Edit(model);
        }

        private Task ValidateAsync(FacebookPluginPart model, IUpdateModel updater)
        {
            return Task.CompletedTask;
        }
    }
}
