using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Modules.Services
{
    public class LocalCulture : ILocalCulture
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private CultureInfo _culture;

        public LocalCulture(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CultureInfo> GetLocalCultureAsync()
        {
            // Caching the result per request
            if (_culture == null)
            {
                _culture = await LoadLocalCultureAsync();
            }

            return _culture;
        }

        private async Task<CultureInfo> LoadLocalCultureAsync()
        {
            var test = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureProvider>();
            var providers = new RequestLocalizationOptions().RequestCultureProviders;
            var cultureResults = new List<CultureInfo>();

            if (providers != null)
            {
                foreach (var provider in providers)
                {
                    var providerCultureResult = await provider.DetermineProviderCultureResult(_httpContextAccessor.HttpContext);
                    if (providerCultureResult == null)
                    {
                        continue;
                    }
                    
                    foreach (var cultureInfo in providerCultureResult.Cultures) {
                        cultureResults.Add(CultureInfo.GetCultureInfo(cultureInfo.ToString()));
                    }
                }
            }

            if (cultureResults.Count == 0)
            {
                return CultureInfo.InvariantCulture;
            }
            //else if (cultureResults.Count > 1)
            //{
            //    cultureResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            //}

            //foreach (var result in cultureResults)
            //{
            //    var value = await result.Name();

            //    if (!String.IsNullOrEmpty(value))
            //    {
            //        return CultureInfo.GetCultureInfo(value);
            //    }
            //}

            return CultureInfo.InvariantCulture;
        }
    }
}
