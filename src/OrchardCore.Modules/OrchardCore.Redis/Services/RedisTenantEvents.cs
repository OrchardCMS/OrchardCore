using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Redis.Services
{
    public class RedisTenantEvents : ModularTenantEvents
    {
        private readonly IRedisService _redis;

        public RedisTenantEvents(IRedisService redis) => _redis = redis;

        public override Task ActivatingAsync() => _redis.ConnectAsync();
    }
}
