namespace OrchardCore.Redis;

public interface IRedisTokenProvider
{
    /// <summary>
    /// Returns dynamic authentication info for Redis.
    /// </summary>
    Task<string> GetTokenAsync();
}
