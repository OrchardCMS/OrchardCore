using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Mappings;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services
{
    public class SmtpSettingsConfiguration : IConfigureOptions<SmtpSettings>
    {
        private readonly ISiteService _site;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public SmtpSettingsConfiguration(
            ISiteService site,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpSettingsConfiguration> logger,
            IMapper mapper)
        {
            _site = site;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
            _mapper = mapper;
        }

        public void Configure(SmtpSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SmtpSettings>();

            options = _mapper.Map<SmtpSettings>(settings);

            // Decrypt the password
            if (!string.IsNullOrWhiteSpace(settings.Password))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
                    options.Password = protector.Unprotect(settings.Password);
                }
                catch
                {
                    _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
                }
            }
        }
    }
}
