using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.ViewModels;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Mvc.ModelBinding;
using Orchard.Settings;
using YesSql;

namespace Orchard.Autoroute.Drivers
{
    public class AutoroutePartDisplay : ContentPartDisplayDriver<AutoroutePart>
    {
        public static char[] InvalidCharactersForPath = ":?#[]@!$&'()*+,.;=<>\\|%".ToCharArray();
        public const int MaxPathLength = 1024;

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly YesSql.ISession _session;
        private readonly IStringLocalizer<AutoroutePartDisplay> T;

        public AutoroutePartDisplay(
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            YesSql.ISession session,
            IStringLocalizer<AutoroutePartDisplay> localizer
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _session = session;
            T = localizer;
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
                    autoroutePart.ContentItem.ContentItemId == homeRoute["contentItemId"]?.ToString())
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

            await ValidateAsync(model, updater);

            return Edit(model);
        }

        private async Task ValidateAsync(AutoroutePart autoroute, IUpdateModel updater)
        {
            if (autoroute.Path?.IndexOfAny(InvalidCharactersForPath) > -1 || autoroute.Path?.IndexOf(' ') > -1)
            {
                var invalidCharactersForMessage = string.Join(", ", InvalidCharactersForPath.Select(c => $"\"{c}\""));
                updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), T["Please do not use any of the following characters in your permalink: {0}. No spaces are allowed (please use dashes or underscores instead).", invalidCharactersForMessage]);
            }

            if (autoroute.Path?.Length > MaxPathLength)
            {
                updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), T["Your permalink is too long. The permalink can only be up to {0} characters.", MaxPathLength]);
            }

            if (autoroute.Path != null && await _session.QueryIndexAsync<AutoroutePartIndex>(o => o.Path == autoroute.Path && o.ContentItemId != autoroute.ContentItem.ContentItemId).Count() > 0)
            {
                updater.ModelState.AddModelError(Prefix, nameof(autoroute.Path), T["Your permalink is already in use."]);
            }
        }
    }
}
