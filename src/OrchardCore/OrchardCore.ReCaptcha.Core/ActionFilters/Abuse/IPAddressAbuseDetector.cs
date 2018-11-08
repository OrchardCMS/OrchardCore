using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace OrchardCore.ReCaptcha.ActionFilters.Abuse
{
    public class IpAddressAbuseDetector : IDetectAbuse
    {
        private const string IpAddressAbuseDetectorCacheKey = "IpAddressAbuseDetector";

        private readonly IMemoryCache _memoryCache;
        private readonly HttpContext _httpContext;

        public IpAddressAbuseDetector(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _memoryCache = memoryCache;
        }

        public void ClearAbuseFlags()
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

        public AbuseDetectResult DetectAbuse()
        {
            var ipAddressKey = GetIpAddressCacheKey(_httpContext);
            var faultyRequestCount = _memoryCache.GetOrCreate<int>(ipAddressKey, fact => 0);

            return new AbuseDetectResult()
            {
                // this should be configurable
                SuspectAbuse = faultyRequestCount > 5
            };
        }

        public void FlagPossibleAbuse()
        {
            var ipAddressKey = GetIpAddressCacheKey(_httpContext);
            
            // this has race conditions, but it's ok
            var faultyRequestCount = _memoryCache.GetOrCreate<int>(ipAddressKey, fact => 0);
            faultyRequestCount++;
            _memoryCache.Set(ipAddressKey, faultyRequestCount);
        }
    }
}
