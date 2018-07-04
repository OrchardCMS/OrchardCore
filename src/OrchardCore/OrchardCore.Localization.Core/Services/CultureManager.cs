using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services
{
    public class CultureManager : ICultureManager
    {
        private readonly ICultureStore _cultureStore;
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;

        public CultureManager(
            ICultureStore cultureStore,
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor,
            ISiteService siteService)
        {
            _cultureStore = cultureStore;
            _distributedCache = distributedCache;
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
        }

        public IEnumerable<CultureRecord> ListCultures()
        {
            return _cultureStore.GetAllCultures().Result;
        }

        public void AddCulture(string cultureName)
        {
            if (!IsValidCulture(cultureName))
            {
                throw new ArgumentException("cultureName");
            }

            if (ListCultures().Any(culture => culture.Culture == cultureName))
            {
                return;
            }

            _cultureStore.SaveAsync(new CultureRecord { Culture = cultureName }, new System.Threading.CancellationToken());
        }

        public void DeleteCulture(string cultureName)
        {
            if (!IsValidCulture(cultureName))
            {
                throw new ArgumentException("cultureName");
            }

            if (ListCultures().Any(culture => culture.Culture == cultureName))
            {
                var culture = ListCultures().Where(cr => cr.Culture == cultureName).FirstOrDefault();
                _cultureStore.DeleteAsync(culture, new System.Threading.CancellationToken());
            }
        }

        public CultureRecord GetCultureById(int id)
        {
            return ListCultures().Where(c => c.Id == id).FirstOrDefault();
        }

        public CultureRecord GetCultureByName(string cultureName)
        {
            return ListCultures().Where(c => c.Culture == cultureName).FirstOrDefault();
        }

        public string GetSiteCulture()
        {
            return _siteService.GetSiteSettingsAsync().Result.Culture;
        }

        public string GetCurrentCulture()
        {
            return _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().Provider != null ? _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.Name : CultureInfo.CurrentCulture.Name;
        }

        public bool IsValidCulture(string cultureName)
        {
            try
            {
                CultureInfo.GetCultureInfo(cultureName);
                if (Regex.IsMatch(cultureName, @"^[a-zA-Z]{1,8}(?:-[a-zA-Z0-9]{1,8})*$"))
                {
                    return true;
                }
                return false;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }

        public bool CultureExist(string cultureName)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
