using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Modules.Services
{
    public class LocalCulture : ILocalCulture
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalCulture(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Returns middleware current RequestCultureProvider culture
        /// else if localization module is not enabled returns CultureInfo.Invariant
        /// </summary>
        /// <returns></returns>
        public Task<CultureInfo> GetLocalCultureAsync()
        {
            return Task.FromResult(_httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture ?? CultureInfo.InvariantCulture);
        }

        public bool IsLocalizationEnabled()
        {
            //TODO need to find a proper way to tell that the Localization module is disabled
            // on a clean install if we have a default culture set to Invariant we will never 
            // be able to edit the localization if we hide the form with this function returned value.
            return _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>() != null && _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().Provider != null;
        }
    }
}
