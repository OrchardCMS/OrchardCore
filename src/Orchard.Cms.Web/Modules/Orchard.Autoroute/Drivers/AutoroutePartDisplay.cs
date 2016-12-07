using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Settings;

namespace Orchard.Autoroute.Drivers
{
    public class AutoroutePartDisplay : ContentPartDisplayDriver<AutoroutePart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private IHttpContextAccessor _httpContextAccessor;

        public AutoroutePartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override IDisplayResult Edit(AutoroutePart autoroutePart)
        {
            return Shape<AutoroutePartViewModel>("AutoroutePart_Edit", async model =>
            {

                model.Path = autoroutePart.Path;
                model.AutoroutePart = autoroutePart;
                model.SetHomepage = false;

                var siteSettings = await _siteService.GetSiteSettingsAsync();
                var homeRoute = siteSettings.HomeRoute;
                if (homeRoute["area"]?.ToString() == "Orchard.Contents" &&
                    homeRoute["controller"]?.ToString() == "Item" &&
                    homeRoute["action"]?.ToString() == "Display" &&
                    autoroutePart.ContentItem.ContentItemId == homeRoute["id"]?.ToString())
                {
                    model.IsHomepage = true;
                }

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(autoroutePart.ContentItem.ContentType);
                var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(AutoroutePart), StringComparison.Ordinal));
                model.Settings = contentTypePartDefinition.Settings.ToObject<AutoroutePartSettings>();
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(AutoroutePart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Path);
            
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext != null && await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.SetHomepage))
            {
                await updater.TryUpdateModelAsync(model, Prefix, t => t.SetHomepage);
            }

            return Edit(model);
        }
    }
}
