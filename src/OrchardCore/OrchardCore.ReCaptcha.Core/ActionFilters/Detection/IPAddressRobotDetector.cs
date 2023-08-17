using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.ActionFilters.Detection
{
    public class IPAddressRobotDetector : IDetectRobots
    {
        private const string IpAddressAbuseDetectorCacheKey = "IpAddressRobotDetector";

        private readonly IMemoryCache _memoryCache;
        private readonly IClientIPAddressAccessor _clientIpAddressAccessor;
        private readonly ReCaptchaSettings _settings;

        public IPAddressRobotDetector(
            IClientIPAddressAccessor clientIpAddressAccessor,
            IMemoryCache memoryCache,
            IOptions<ReCaptchaSettings> settingsAccessor)
        {
            _clientIpAddressAccessor = clientIpAddressAccessor;
            _memoryCache = memoryCache;
            _settings = settingsAccessor.Value;
        }

        public void IsNotARobot()
        {
            var ipAddressKey = GetIpAddressCacheKey();
            _memoryCache.Remove(ipAddressKey);
        }

        private string GetIpAddressCacheKey()
        {
            var address = _clientIpAddressAccessor.GetIPAddressAsync().GetAwaiter().GetResult();

            return $"{IpAddressAbuseDetectorCacheKey}:{address?.ToString() ?? String.Empty}";
        }

        public RobotDetectionResult DetectRobot()
        {
            var ipAddressKey = GetIpAddressCacheKey();
            var faultyRequestCount = _memoryCache.GetOrCreate(ipAddressKey, fact => 0);

            return new RobotDetectionResult()
            {
                IsRobot = faultyRequestCount > _settings.DetectionThreshold
            };
        }

        public void FlagAsRobot()
        {
            var ipAddressKey = GetIpAddressCacheKey();

            // This has race conditions, but it's ok.
            var faultyRequestCount = _memoryCache.GetOrCreate(ipAddressKey, fact => 0);
            faultyRequestCount++;
            _memoryCache.Set(ipAddressKey, faultyRequestCount);
        }
    }
}
