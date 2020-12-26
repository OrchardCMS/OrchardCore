using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Deployment
{
    public class AllShortcodesDeploymentSource : IDeploymentSource
    {
        private readonly ShortcodeTemplatesManager _templatesManager;

        public AllShortcodesDeploymentSource(ShortcodeTemplatesManager templatesManager)
        {
            _templatesManager = templatesManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allTemplatesStep = step as AllShortcodesDeploymentStep;

            if (allTemplatesStep == null)
            {
                return;
            }

            var templateObjects = new JObject();
            var templates = await _templatesManager.GetShortcodeTemplatesDocumentAsync();

            foreach (var template in templates.ShortcodeTemplates)
            {
                templateObjects[template.Key] = JObject.FromObject(template.Value);
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Shortcodes"),
                new JProperty("Shortcodes", templateObjects)
            ));
        }
    }
}
