using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.ActionFilters.Detection;

public class IPAddressRobotDetector : IDetectRobots
{
    private const string IpAddressAbuseDetectorCacheKey = "IpAddressRobotDetector";

    private readonly IDistributedCache _distributedCache;
    private readonly IClientIPAddressAccessor _clientIpAddressAccessor;
    private readonly ReCaptchaSettings _settings;

    public IPAddressRobotDetector(
        IClientIPAddressAccessor clientIpAddressAccessor,
        IDistributedCache distributedCache,
        IOptions<ReCaptchaSettings> settingsAccessor)
    {
        _clientIpAddressAccessor = clientIpAddressAccessor;
        _distributedCache = distributedCache;
        _settings = settingsAccessor.Value;
    }

    public async ValueTask IsNotARobotAsync(string tag)
    {
        var ipAddressKey = await GetIpAddressCacheKeyAsync();

        var result = await GetCacheAsync(ipAddressKey);

        if (result?.Data is not null && result.Data.Remove(tag))
        {
            await SetCacheAsync(ipAddressKey, result);
        }
    }

    public async ValueTask<RobotDetectionResult> DetectRobotAsync(string tag)
    {
        var ipAddressKey = await GetIpAddressCacheKeyAsync();

        var result = await GetCacheAsync(ipAddressKey);

        var faultyRequestCount = 0;

        result?.Data?.TryGetValue(tag, out faultyRequestCount);

        return new RobotDetectionResult()
        {
            IsRobot = faultyRequestCount >= _settings.DetectionThreshold
        };
    }

    public async ValueTask FlagAsRobotAsync(string tag)
    {
        var ipAddressKey = await GetIpAddressCacheKeyAsync();

        var result = await GetCacheAsync(ipAddressKey);

        // This has race conditions, but it's ok.
        if (result != null)
        {
            var counter = 0;

            result.Data?.TryGetValue(tag, out counter);

            result.Data[tag] = ++counter;
        }
        else
        {
            result = new RobotCacheCounter();
            result.Data.Add(tag, 1);
        }

        await SetCacheAsync(ipAddressKey, result);
    }

    private async Task<RobotCacheCounter> GetCacheAsync(string ipAddressKey)
    {
        var data = await _distributedCache.GetAsync(ipAddressKey);

        if (data != null)
        {
            return JsonSerializer.Deserialize<RobotCacheCounter>(data);
        }

        return null;
    }

    private Task SetCacheAsync(string ipAddressKey, RobotCacheCounter result)
    {
        return _distributedCache.SetAsync(ipAddressKey, JsonSerializer.SerializeToUtf8Bytes(result), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
        });
    }

    private async ValueTask<string> GetIpAddressCacheKeyAsync()
    {
        var address = await _clientIpAddressAccessor.GetIPAddressAsync();

        return $"{IpAddressAbuseDetectorCacheKey}:{address?.ToString() ?? string.Empty}";
    }
}

public class RobotCacheCounter
{
    public Dictionary<string, int> Data { set; get; }
}
