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

        public Task<CultureInfo> GetLocalCultureAsync()
        {
            return Task.FromResult(_httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture ?? CultureInfo.CurrentCulture);
        }
    }
}
