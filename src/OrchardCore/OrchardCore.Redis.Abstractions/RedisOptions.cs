using StackExchange.Redis;

namespace OrchardCore.Redis
{
    /// <summary>
    /// Redis options.
    /// </summary>
    public class RedisOptions
    {
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
