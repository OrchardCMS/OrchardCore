using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Deployment
{
    public class AllShortcodeTemplatesDeploymentSource : IDeploymentSource
    {
        private readonly ShortcodeTemplatesManager _templatesManager;

        public AllShortcodeTemplatesDeploymentSource(ShortcodeTemplatesManager templatesManager)
        {
            _templatesManager = templatesManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allTemplatesStep = step as AllShortcodeTemplatesDeploymentStep;

            if (allTemplatesStep == null)
            {
                return;
            }

            var templateObjects = new JsonObject();
            var templates = await _templatesManager.GetShortcodeTemplatesDocumentAsync();

            foreach (var template in templates.ShortcodeTemplates)
            {
                templateObjects[template.Key] = JObject.FromObject(template.Value);
            }

            result.Steps.Add(new JsonObject
            {
                ["name"] = "ShortcodeTemplates",
                ["ShortcodeTemplates"] = templateObjects,
            });
        }
    }
}
