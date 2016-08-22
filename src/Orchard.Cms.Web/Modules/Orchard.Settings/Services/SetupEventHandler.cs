using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Orchard.Settings.Services
{
    public interface ISetupEventHandler : IEventHandler
    {
        Task Setup(string siteName, string userName);
    }

    /// <summary>
    /// During setup, registers the Super User.
    /// </summary>
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SetupEventHandler(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public async Task Setup(string siteName, string userName)
        {
            var siteService = _serviceProvider.GetRequiredService<ISiteService>();

            // Updating site settings
            var siteSettings = await siteService.GetSiteSettingsAsync();
            siteSettings.SiteName = siteName;
            siteSettings.SuperUser = userName;            
            siteSettings.BaseUrl = UriHelper.GetDisplayUrl(_httpContextAccessor.HttpContext.Request);
            await siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in
        }
    }
}
