using System.Threading.Tasks;

namespace OrchardCore.Redis
{
    public static class RedisServiceExtensions
    {
        public static bool IsConnected(this IRedisService redis) => redis.Database != null;

        public static Task EnsureConnectedAsync(this IRedisService redis)
        {
            if (redis.Database != null)
            {
                return Task.CompletedTask;
            }

            return redis.ConnectAsync();
        }
    }
}
