using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.ActionFilters.Abuse;
using OrchardCore.ReCaptcha.Configuration;

namespace OrchardCore.ReCaptcha.Services
{
    public class ReCaptchaService
    {
        private readonly ReCaptchaClient _reCaptchaClient;
        private readonly ReCaptchaSettings _settings;
        private readonly IEnumerable<IDetectAbuse> _abuseDetection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReCaptchaService> _logger;

        public ReCaptchaService(ReCaptchaClient reCaptchaClient, IOptions<ReCaptchaSettings> optionsAccessor, IEnumerable<IDetectAbuse> abuseDetection, IHttpContextAccessor httpContextAccessor, ILogger<ReCaptchaService> logger)    
        {
            _reCaptchaClient = reCaptchaClient;
            _settings = optionsAccessor.Value;
            _abuseDetection = abuseDetection;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Flags the behavior of the request as suspect, internal implementations will decide if the behavior is convicted 
        /// </summary>
        /// <returns></returns>
        public void FlagAsSuspect()
        {
            _abuseDetection.Invoke(i => i.FlagPossibleAbuse(), _logger);
        }

        /// <summary>
        /// Determines if the request has been convicted as abuse
        /// </summary>
        /// <returns></returns>
        public bool IsConvicted()
        {
            var result = _abuseDetection.Invoke(i => i.DetectAbuse(), _logger);
            return result.Any(a => a.SuspectAbuse);
        }

        /// <summary>
        /// Clears all convictions and resets all flags
        /// </summary>
        /// <returns></returns>
        public void MarkAsInnocent()
        {
            _abuseDetection.Invoke(i => i.ClearAbuseFlags(), _logger);
        }

        /// <summary>
        /// Verifies the ReCaptcha response with the ReCaptcha webservice
        /// </summary>
        /// <param name="reCaptchaResponse"></param>
        /// <returns></returns>
        public async Task<bool> VerifyCaptchaResponseAsync(string reCaptchaResponse)
        {
            return !String.IsNullOrWhiteSpace(reCaptchaResponse) && await _reCaptchaClient.VerifyAsync(reCaptchaResponse, _settings.SecretKey);
        }
    }
}
