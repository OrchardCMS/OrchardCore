using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;

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

        public SetupEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Setup(string siteName, string userName)
        {
            var siteService = _serviceProvider.GetRequiredService<ISiteService>();

            // Updating site settings
            var siteSettings = await siteService.GetSiteSettingsAsync();
            siteSettings.SiteName = siteName;
            siteSettings.SuperUser = userName;
            await siteService.UpdateSiteSettingsAsync(siteSettings);

            // TODO: Add Encryption Settings in
        }
    }
}