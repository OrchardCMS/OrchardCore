using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Deployment
{
    public class AllTemplatesDeploymentSource : IDeploymentSource
    {
        private readonly TemplatesManager _templatesManager;

        public AllTemplatesDeploymentSource(TemplatesManager templatesManager)
        {
            _templatesManager = templatesManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allTemplatesState = step as AllTemplatesDeploymentStep;

            if (allTemplatesState == null)
            {
                return;
            }

            var templateObjects = new JObject();
            var templates = await _templatesManager.GetTemplatesDocumentAsync();

            foreach (var template in templates.Templates)
            {
                templateObjects[template.Key] = JObject.FromObject(template.Value);
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Templates"),
                new JProperty("Templates", templateObjects)
            ));
        }
    }
}
