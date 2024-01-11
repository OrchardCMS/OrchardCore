using StackExchange.Redis;

namespace OrchardCore.Redis
{
    /// <summary>
    /// Configuration options for <see cref="IRedisService"/>.
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// The configuration string used to connect to Redis.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        /// <summary>
        /// Prefix alowing a Redis instance to be shared.
        /// </summary>
        public string InstancePrefix { get; set; }
    }
}
