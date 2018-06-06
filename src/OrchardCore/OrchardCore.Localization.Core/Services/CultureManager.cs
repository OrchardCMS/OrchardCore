using System;
using System.Collections.Generic;
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
            _cultureStore.SaveAsync(new CultureRecord{ Culture = cultureName }, new System.Threading.CancellationToken());
        }

        public void DeleteCulture(string cultureName)
        {
            _cultureStore.DeleteAsync(new CultureRecord { Culture = cultureName }, new System.Threading.CancellationToken());
        }

        public CultureRecord GetCultureById(int id)
        {
            throw new NotImplementedException();
        }

        public CultureRecord GetCultureByName(string cultureName)
        {
            throw new NotImplementedException();
        }

        public string GetSiteCulture()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentCulture(HttpContext requestContext)
        {
            throw new NotImplementedException();
        }

        // "<languagecode2>" or
        // "<languagecode2>-<country/regioncode2>" or
        // "<languagecode2>-<scripttag>-<country/regioncode2>"
        public bool IsValidCulture(string cultureName) {
            var segments = cultureName.Split('-');

            if (segments.Length == 0) {
                return false;
            }

            if (segments.Length > 3) {
                return false;
            }

            if (segments.Any(s => s.Length < 2)) {
                return false;
            }

            return true;
        }

    }
}
