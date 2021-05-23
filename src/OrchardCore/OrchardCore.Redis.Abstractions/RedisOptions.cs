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
        /// Prefix alowing unrelated tenants to share a Redis instance.
        /// </summary>
        public string InstancePrefix { get; set; }
    }
}
