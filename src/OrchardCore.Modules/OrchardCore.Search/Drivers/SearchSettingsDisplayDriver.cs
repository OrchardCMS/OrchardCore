using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Model;
using OrchardCore.Search.ViewModels;
using OrchardCore.Settings;
using System.Linq;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Search.Drivers
{
    public class SearchSettingsDisplayDriver : SectionDisplayDriver<ISite, SearchSettings>
    {
        public const string GroupId = "search";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public SearchSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider,
            IShellHost shellHost,
            ShellSettings shellSettings
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _serviceProvider = serviceProvider;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
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
                var searchProviders = _serviceProvider.GetServices<SearchProvider>();

                if(searchProviders.Any())
                {
                    model.SearchProviders = searchProviders.Select(x => x.Name);
                }
                
                model.SearchProvider = settings.SearchProvider;
            }).Location("Content:2").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(SearchSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSearchSettings))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
                var model = new SearchSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.SearchProvider = model.SearchProvider;
            }

            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            return await EditAsync(section, context);
        }
    }
}
