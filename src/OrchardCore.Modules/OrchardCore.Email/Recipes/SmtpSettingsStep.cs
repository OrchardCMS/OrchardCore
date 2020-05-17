using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Email;
using OrchardCore.Entities;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Email.Recipes
{
    /// <summary>
    /// This recipe step sets general login settings.
    /// </summary>
    public class SmtpSettingsStep : IRecipeStepHandler
    {
        private readonly ISiteService _siteService;

        public SmtpSettingsStep(ISiteService siteService)
            => _siteService = siteService;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, nameof(SmtpSettings), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<SmtpSettingsStepModel>();

            var site = await _siteService.LoadSiteSettingsAsync();
            var settings = site.As<SmtpSettings>();
            settings.DefaultSender = model.DefaultSender;
            settings.DeliveryMethod = model.DeliveryMethod;
            settings.PickupDirectoryLocation = model.PickupDirectoryLocation;
            settings.Host = model.Host;
            settings.Port = model.Port;
            settings.AutoSelectEncryption = model.AutoSelectEncryption;
            settings.RequireCredentials = model.RequireCredentials;
            settings.UseDefaultCredentials = model.UseDefaultCredentials;
            settings.EncryptionMethod = model.EncryptionMethod;
            settings.UserName = model.UserName;
            settings.Password = model.Password;
            site.Put<SmtpSettings>(settings);
            await _siteService.UpdateSiteSettingsAsync(site);
        }
    }
}
