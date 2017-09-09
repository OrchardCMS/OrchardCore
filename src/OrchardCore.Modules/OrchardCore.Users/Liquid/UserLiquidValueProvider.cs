using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class UserLiquidValueProvider : ILiquidValueProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLiquidValueProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task PopulateValuesAsync(IDictionary<string, object> values)
        {
            values.Add("User", _httpContextAccessor.HttpContext.User);
            values.Add("User.Identity", _httpContextAccessor.HttpContext.User.Identity);
            return Task.CompletedTask;
        }
    }
}
