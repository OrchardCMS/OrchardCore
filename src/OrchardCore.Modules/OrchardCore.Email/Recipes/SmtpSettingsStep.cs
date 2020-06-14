using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Email.Recipes
{
    public class SmtpSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private ILogger _logger;

        public SmtpSettingsStep(ISiteService siteService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<SmtpSettingsStep> logger
            )
        {
            _siteService = siteService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "SmtpSettings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var site = await _siteService.LoadSiteSettingsAsync();
            // Retrieve the curent properties first and merge them with the step values so we don't replace an ignored value.
            var jSettings = site.Properties["SmtpSettings"] as JObject;
            if (jSettings != null)
            {
                jSettings.Merge(context.Step["SmtpSettings"]);
            }

            var model = jSettings.ToObject<SmtpSettings>();

            var passwordProperty = context.Properties.SelectToken("SmtpSettings.Password")?.ToObject<Property>();

            if (passwordProperty != null)
            {
                switch (passwordProperty.Handler)
                {
                    case PropertyHandler.UserSupplied:
                        if (!String.IsNullOrEmpty(passwordProperty.Value))
                        {
                            // Encrypt the password as it was supplied in plain text by the user.
                            model.Password = EncryptPassword(passwordProperty.Value);
                        }
                        else
                        {
                            _logger.LogError("User supplied setting 'Password' not provided for 'SmtpSettings'");
                        }
                        break;
                    case PropertyHandler.PlainText:
                        // Encrypt the password as it was supplied in plain text.
                        model.Password = EncryptPassword(passwordProperty.Value);
                        break;
                    case PropertyHandler.Encrypted:
                        if (!String.IsNullOrEmpty(passwordProperty.Value))
                        {
                            try
                            {
                                var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
                                // Decrypt the password to test if the encryption keys are valid.
                                protector.Unprotect(passwordProperty.Value);
                                // On success we can store the supplied value.
                                model.Password = passwordProperty.Value;
                            }
                            catch
                            {
                                _logger.LogError("The Smtp password could not be decrypted. It may have been encrypted using a different key.");
                            }
                        }
                        break;
                    default:
                        break;

                }
            }

            site.Put(model);
            await _siteService.UpdateSiteSettingsAsync(site);
        }

        private string EncryptPassword(string password)
        {
            var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
            return protector.Protect(password);
        }
    }
}
