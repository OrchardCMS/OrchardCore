using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Core.Settings.Services
{
    public class SiteService : ISiteService
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public SiteService(
            ISession session,
            IContentManager contentManager)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public async Task<ISite> GetSiteSettingsAsync()
        {
            var site = await _session.QueryAsync<ContentItem, ContentItemIndex>(x => x.ContentType == "Site").FirstOrDefault();
            
            if (site == null)
            {
                site = _contentManager.New("Site");
                site.Weld(new SiteSettingsPart()
                {

                    SiteSalt = Guid.NewGuid().ToString("N"),
                    SiteName = "My Orchard Project Application",
                    PageTitleSeparator = " - ",
                    TimeZone = TimeZoneInfo.Local.Id
                });

                _contentManager.Create(site);
            }

            return site.As<SiteSettingsPart>();
        }
    }
}
