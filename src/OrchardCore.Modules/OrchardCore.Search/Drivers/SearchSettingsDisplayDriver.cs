using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Models;
using OrchardCore.Search.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Drivers
{
    public class SearchSettingsDisplayDriver : SectionDisplayDriver<ISite, SearchSettings>
    {
        public const string GroupId = "search";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceProvider _serviceProvider;

        public SearchSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _serviceProvider = serviceProvider;
        }

        public override async Task<IDisplayResult> EditAsync(SearchSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSearchSettings))
            {
                return null;
            }

            return Initialize<SearchSettingsViewModel>("SearchSettings_Edit", model =>
            {
                var searchServices = _serviceProvider.GetServices<ISearchService>();

                model.SearchServices = searchServices.Select(service => new SelectListItem(service.Name, service.Name)).ToList();
                model.Placeholder = settings.Placeholder;
                model.PageTitle = settings.PageTitle;
                model.ProviderName = settings.ProviderName;
            }).Location("Content:2").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SearchSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSearchSettings))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new SearchSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    section.ProviderName = model.ProviderName;
                    section.Placeholder = model.Placeholder;
                    section.PageTitle = model.PageTitle;
                }
            }

            return await EditAsync(section, context);
        }
    }
}
