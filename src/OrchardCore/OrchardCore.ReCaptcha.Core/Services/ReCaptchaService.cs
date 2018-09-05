using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.ReCaptcha.Core.ActionFilters.Abuse;
using OrchardCore.ReCaptcha.Core.Configuration;

namespace OrchardCore.ReCaptcha.Core.Services
{
    public class ReCaptchaService : IReCaptchaService
    {
        private readonly IReCaptchaClient _reCaptchaClient;
        private readonly ReCaptchaSettings _settings;
        private readonly IEnumerable<IDetectAbuse> _abuseDetection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReCaptchaService> _logger;

        public ReCaptchaService(IReCaptchaClient reCaptchaClient, IOptions<ReCaptchaSettings> optionsAccessor, IEnumerable<IDetectAbuse> abuseDetection, IHttpContextAccessor httpContextAccessor, ILogger<ReCaptchaService> logger)    
        {
            _reCaptchaClient = reCaptchaClient;
            _settings = optionsAccessor.Value;
            _abuseDetection = abuseDetection;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Task FlagAsSuspectAsync()
        {
            _abuseDetection.Invoke(i => i.FlagPossibleAbuse(_httpContextAccessor.HttpContext), _logger);

            return Task.CompletedTask;
        }

        public Task<bool> IsConvictedAsync()
        {
            var result = _abuseDetection.Invoke(i => i.DetectAbuse(_httpContextAccessor.HttpContext), _logger);
            return Task.FromResult(result.Any(a => a.SuspectAbuse));
        }

        public Task MarkAsInnocentAsync()
        {
            _abuseDetection.Invoke(i => i.ClearAbuseFlags(_httpContextAccessor.HttpContext), _logger);

            return Task.CompletedTask;
        }

        public async Task<bool> VerifyCaptchaResponseAsync(string reCaptchaResponse)
        {
            var isValid = !String.IsNullOrWhiteSpace(reCaptchaResponse);
            
            if(isValid)
                isValid = await _reCaptchaClient.VerifyAsync(reCaptchaResponse, _settings.SecretKey);

            return isValid;
        }
    }
}
