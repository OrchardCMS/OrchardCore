using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Models
{
    public class SmtpSettings
    {
        [Required( AllowEmptyStrings = false ), EmailAddress]

        public string DefaultSender { get; set; }
        [Required( AllowEmptyStrings = false )]
        public string Host { get; set; }
        [Range(0,65535)]
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; }
        public bool RequireCredentials { get; set; }
        public bool UseDefaultCredentials { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class SmtpSettingsConfiguration : IConfigureOptions<SmtpSettings>
    {
        private readonly ISiteService _site;

        public SmtpSettingsConfiguration(ISiteService site)
        {
            _site = site;
        }

        public void Configure(SmtpSettings options)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SmtpSettings>();

            options.DefaultSender = settings.DefaultSender;
            options.Host = settings.Host;
            options.Port = settings.Port;
            options.EnableSsl = settings.EnableSsl;
            options.RequireCredentials = settings.RequireCredentials;
            options.UseDefaultCredentials = settings.UseDefaultCredentials;
            options.UserName = settings.UserName;
            options.Password = settings.Password;
        }
    }
}
