using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.ActionFilters.Detection
{
    public class IpAddressRobotDetector : IDetectRobots
    {
        private const string IpAddressAbuseDetectorCacheKey = "IpAddressRobotDetector";

        private readonly IMemoryCache _memoryCache;
        private readonly HttpContext _httpContext;
        private readonly ReCaptchaSettings _settings;

        public IpAddressRobotDetector(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, IOptions<ReCaptchaSettings> settingsAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _memoryCache = memoryCache;
            _settings = settingsAccessor.Value;
        }

        public void IsNotARobot()
        {
            var ipAddressKey = GetIpAddressCacheKey(_httpContext);
            _memoryCache.Remove(ipAddressKey);
        }

        private string GetIpAddressCacheKey(HttpContext context)
        {
            return $"{IpAddressAbuseDetectorCacheKey}:{GetIpAddress()}";
        }

        private string GetIpAddress()
        {
            return _httpContext.Connection.RemoteIpAddress.ToString();
        }

        public RobotDetectionResult DetectRobot()
        {
            var ipAddressKey = GetIpAddressCacheKey(_httpContext);
            var faultyRequestCount = _memoryCache.GetOrCreate<int>(ipAddressKey, fact => 0);

            return new RobotDetectionResult()
            {
                IsRobot = faultyRequestCount > _settings.DetectionThreshold
            };
        }

        public void FlagAsRobot()
        {
            var ipAddressKey = GetIpAddressCacheKey(_httpContext);

            // this has race conditions, but it's ok
            var faultyRequestCount = _memoryCache.GetOrCreate<int>(ipAddressKey, fact => 0);
            faultyRequestCount++;
            _memoryCache.Set(ipAddressKey, faultyRequestCount);
        }
    }
}
