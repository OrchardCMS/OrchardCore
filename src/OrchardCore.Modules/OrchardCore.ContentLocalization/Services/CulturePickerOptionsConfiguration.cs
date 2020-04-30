using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.ContentLocalization.Services
{
    public class CulturePickerOptionsConfiguration : IConfigureOptions<CulturePickerOptions>
    {
        public static readonly int DefaultCookieLifeTime = 14;

        private readonly IShellConfiguration _shellConfiguration;

        public CulturePickerOptionsConfiguration(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public void Configure(CulturePickerOptions options)
        {
            var section = _shellConfiguration.GetSection("OrchardCore_ContentLocalization:CulturePicker");

            options.CookieLifeTime = section.GetValue("CookieLifeTime", DefaultCookieLifeTime);
        }
    }
}
