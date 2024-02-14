using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Deployment
{
    public class AllShortcodeTemplatesDeploymentSource : IDeploymentSource
    {
        private readonly ShortcodeTemplatesManager _templatesManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public AllShortcodeTemplatesDeploymentSource(
            ShortcodeTemplatesManager templatesManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _templatesManager = templatesManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
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
                templateObjects[template.Key] = JObject.FromObject(template.Value, _jsonSerializerOptions);
            }

            result.Steps.Add(new JsonObject
            {
                ["name"] = "ShortcodeTemplates",
                ["ShortcodeTemplates"] = templateObjects,
            });
        }
    }
}
