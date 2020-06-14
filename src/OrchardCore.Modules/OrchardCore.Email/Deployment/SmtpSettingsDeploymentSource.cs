using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Deployment
{
    public class SmtpSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISiteService _siteService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private ILogger _logger;

        public SmtpSettingsDeploymentSource(ISiteService siteService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpSettingsDeploymentSource> logger)
        {
            _siteService = siteService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var smtpSettingsStep = step as SmtpSettingsDeploymentStep;

            if (smtpSettingsStep == null)
            {
                return;
            }

            var smtpSettings = (await _siteService.GetSiteSettingsAsync()).As<SmtpSettings>();

            var jObject = JObject.FromObject(smtpSettings);

            jObject.Remove("Password");

            if (smtpSettingsStep.Password != PropertyHandler.Ignored)
            {
                // Default to user supplied to cover error handling if protector is unsuccessful.
                var passwordProperty = new Property
                {
                    Handler = PropertyHandler.UserSupplied,
                    Value = JToken.FromObject(String.Empty)
                };

                switch (smtpSettingsStep.Password)
                {
                    case PropertyHandler.Encrypted:
                        passwordProperty = new Property
                        {
                            Handler = PropertyHandler.Encrypted,
                            Value = JToken.FromObject(smtpSettings.Password)
                        };
                        break;
                    case PropertyHandler.PlainText:
                        if (!String.IsNullOrWhiteSpace(smtpSettings.Password))
                        {
                            try
                            {
                                var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
                                passwordProperty = new Property
                                {
                                    Handler = PropertyHandler.PlainText,
                                    Value = JToken.FromObject(smtpSettings.Password)

                                    // Value = new JObject(new JProperty("Password", protector.Unprotect(smtpSettings.Password)))
                                };
                            }
                            catch
                            {
                                _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
                            }
                        }
                        break;
                    default:
                        break;

                };


                result.Properties["SmtpSettings"] = new JObject(
                    new JProperty("Password", JObject.FromObject(passwordProperty)),
                    new JProperty("Test", JObject.FromObject(passwordProperty))
                );
                result.Properties["OtherSettings"] = new JObject(
                      new JProperty("Password", JObject.FromObject(passwordProperty)),
                      new JProperty("Test", JObject.FromObject(passwordProperty))
                  );
            }

            // Adding Smtp settings
            result.Steps.Add(new JObject(
                new JProperty("name", "SmtpSettings"),
                new JProperty("SmtpSettings", jObject)
            ));
        }
    }
}
