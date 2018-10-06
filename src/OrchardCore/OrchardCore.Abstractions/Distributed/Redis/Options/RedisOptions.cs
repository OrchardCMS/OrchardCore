using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Options
{
    public class RedisOptions
    {
        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }
    }
}
