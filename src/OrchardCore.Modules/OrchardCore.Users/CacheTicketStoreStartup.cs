using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Users.Authentication
{
    [Feature("OrchardCore.Users.Authentication.CacheTicketStore")]
    public class CacheTicketStoreStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureOptions<CookieAuthenticationOptionsConfigure>();
        }
    }

    public class CookieAuthenticationOptionsConfigure : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieAuthenticationOptionsConfigure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(string name, CookieAuthenticationOptions options)
        {
            if (name == IdentityConstants.ApplicationScheme)
            {
                options.SessionStore = new CacheTicketStore(_httpContextAccessor);
            }
        }

        public void Configure(CookieAuthenticationOptions options)
        {
        }
    }
}
