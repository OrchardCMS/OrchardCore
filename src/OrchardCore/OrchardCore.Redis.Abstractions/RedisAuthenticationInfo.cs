namespace OrchardCore.Redis;

public class RedisAuthenticationInfo
{
    /// <summary>
    /// Password or token to use with Redis.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Username if required by some providers.
    /// </summary>
    public string User { get; set; }
}
