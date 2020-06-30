using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment
{
    public class AllSiteSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllSiteSettingsDeploymentStep>
    {
        private readonly IHtmlLocalizer H;

        public AllSiteSettingsDeploymentStepDriver(IHtmlLocalizer<AllSiteSettingsDeploymentStepDriver> h)
        {
            H = h;
        }

        public override IDisplayResult Display(AllSiteSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("AllSiteSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllSiteSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllSiteSettingsDeploymentStep step)
        {
            var availableSettings = new List<SiteSetting>();
            availableSettings.Add(new SiteSetting("AdminSettings", H["Admin settings"]));
            availableSettings.Add(new SiteSetting("BackgroundTaskSettings", H["Background Task settings"]));
            availableSettings.Add(new SiteSetting("ChangeEmailSettings", H["Change Email settings"]));
            availableSettings.Add(new SiteSetting("FacebookSettings", H["Facebook settings"]));
            availableSettings.Add(new SiteSetting("FacebookLoginSettings", H["Facebook Login settings"]));
            availableSettings.Add(new SiteSetting("GitHubAuthenticationSettings", H["GitHub Authentication settings"]));
            availableSettings.Add(new SiteSetting("GoogleAnalyticsSettings", H["Google Analytics settings"]));
            availableSettings.Add(new SiteSetting("GoogleAuthenticationSettings", H["Google Authentication settings"]));
            availableSettings.Add(new SiteSetting("GraphQLSettings", H["GraphQL settings"]));
            //availableSettings.Add(new SiteSetting("LayerSettings", H["Layer settings"]));
            availableSettings.Add(new SiteSetting("LoginSettings", H["Login settings"]));
            availableSettings.Add(new SiteSetting("MicrosoftAccountSettings", H["Microsoft Account settings"]));
            availableSettings.Add(new SiteSetting("AzureADSettings", H["Microsoft Azure AD settings"]));
            availableSettings.Add(new SiteSetting("OpenIdClientSettings", H["OpenId Client settings"]));
            availableSettings.Add(new SiteSetting("OpenIdServerSettings", H["OpenId Server settings"]));
            availableSettings.Add(new SiteSetting("OpenIdValidationSettings", H["OpenId Validation settings"]));
            availableSettings.Add(new SiteSetting("RegistrationSettings", H["Registration settings"]));
            availableSettings.Add(new SiteSetting("ResetPasswordSettings", H["Reset Password settings"]));
            availableSettings.Add(new SiteSetting("ReCaptchaSettings", H["ReCaptcha settings"]));
            availableSettings.Add(new SiteSetting("ReverseProxySettings", H["Reverse Proxy settings"]));
            availableSettings.Add(new SiteSetting("TwitterSettings", H["Twitter settings"]));
            availableSettings.Add(new SiteSetting("TwitterSigninSettings", H["Twitter Signin settings"]));

            return Initialize<AllSiteSettingsDeploymentStepViewModel>("AllSiteSettingsDeploymentStep_Fields_Edit", model =>
            {
                model.AvailableSettings = availableSettings;
                model.Settings = step.Settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(AllSiteSettingsDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            step.Settings = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step, Prefix, x => x.Settings);

            return Edit(step);
        }
    }
}
