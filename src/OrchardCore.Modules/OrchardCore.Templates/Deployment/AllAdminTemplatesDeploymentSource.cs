using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Deployment
{
    public class AllAdminTemplatesDeploymentSource : IDeploymentSource
    {
        private readonly AdminTemplatesManager _templatesManager;

        public AllAdminTemplatesDeploymentSource(AdminTemplatesManager templatesManager)
        {
            _templatesManager = templatesManager;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allTemplatesState = step as AllAdminTemplatesDeploymentStep;

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
                new JProperty("name", "AdminTemplates"),
                new JProperty("AdminTemplates", templateObjects)
            ));
        }
    }
}
