using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Localization.Models;
using OrchardCore.Modules.Services;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services
{
    public class CultureManager : ICultureManager
    {
        private const string CacheKey = "SiteCultures";
        private readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(1);

        private readonly ICultureStore _cultureStore;
        private readonly IDistributedCache _distributedCache;
        private readonly ILocalCulture _localCulture;
        private readonly ISiteService _siteService;

        public CultureManager(
            ICultureStore cultureStore,
            IDistributedCache distributedCache,
            ILocalCulture localCulture,
            ISiteService siteService)
        {
            _cultureStore = cultureStore;
            _distributedCache = distributedCache;
            _localCulture = localCulture;
            _siteService = siteService;
        }

        public IEnumerable<Culture> ListCultures()
        {
            return _cultureStore.GetCultureRecordAsync().Result.Cultures;
        }

        public void AddCulture(string cultureName)
        {
            if (!IsValidCulture(cultureName))
            {
                throw new ArgumentException("cultureName");
            }

            _cultureStore.SaveAsync(cultureName, new System.Threading.CancellationToken());
        }

        public void DeleteCulture(string cultureName)
        {
            if (!IsValidCulture(cultureName))
            {
                throw new ArgumentException("cultureName");
            }

            _cultureStore.DeleteAsync(cultureName, new System.Threading.CancellationToken());
        }

        public string GetSiteCulture()
        {
            var result = _siteService.GetSiteSettingsAsync().Result.Culture;
            return result ?? CultureInfo.InvariantCulture.Name;
        }

        public string GetCurrentCulture()
        {
            return _localCulture.GetLocalCultureAsync().Result.Name;
        }

        public bool IsValidCulture(string cultureName)
        {
            try
            {
                CultureInfo.GetCultureInfo(cultureName);
                return Regex.IsMatch(cultureName, @"^[a-zA-Z]{1,8}(?:-[a-zA-Z0-9]{1,8})*$") ? true : false;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }

        public bool CultureExist(string cultureName)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => String.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
