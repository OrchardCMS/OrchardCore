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

        private Task<CultureInfo> LoadLocalCultureAsync()
        {
            return Task.FromResult(_httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture);
        }
    }
}
