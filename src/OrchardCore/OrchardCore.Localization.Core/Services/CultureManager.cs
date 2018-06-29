using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public class CultureManager : ICultureManager {
        private readonly ICultureStore _cultureStore;
        private readonly IDistributedCache _distributedCache;

        public CultureManager(
            ICultureStore cultureStore,
            IDistributedCache distributedCache) {
            _cultureStore = cultureStore;
            _distributedCache = distributedCache;
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

            _cultureStore.SaveAsync(new CultureRecord{ Culture = cultureName }, new System.Threading.CancellationToken());
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
            throw new NotImplementedException();
        }

        public string GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public bool IsValidCulture(string cultureName) {
            try
            {
                CultureInfo.GetCultureInfo(cultureName);
                return true;
            }
            catch(CultureNotFoundException) {
                return false;
            }
        }

        public bool CultureExist(string cultureName)
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Any(culture => string.Equals(culture.Name, cultureName, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
