using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Email.Services;
using OrchardCore.Settings;

namespace OrchardCore.Email.Deployment
{
    public class SmtpSettingsDeploymentSource : IDeploymentSource
    {
        private readonly ISmtpService _smtpService;

        public SmtpSettingsDeploymentSource(ISmtpService smtpService)
        {
            _smtpService = smtpService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var smtpSettingsStep = step as SmtpSettingsDeploymentStep;

            if (smtpSettingsStep == null)
            {
                return;
            }

            var smtpSettings = await _smtpService.GetSmtpSettingsAsync();

            // Adding Smtp settings
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("SmtpSettings", JObject.FromObject(smtpSettings))
            ));
        }
    }
}
