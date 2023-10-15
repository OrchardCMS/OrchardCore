using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.Templates.Models;
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
            var allTemplatesStep = step as AllAdminTemplatesDeploymentStep;

            if (allTemplatesStep == null)
            {
                return;
            }

            var templateObjects = new JsonObject();
            var templates = await _templatesManager.GetTemplatesDocumentAsync();

            if (allTemplatesStep.ExportAsFiles)
            {
                foreach (var template in templates.Templates)
                {
                    var fileName = "AdminTemplates/" + template.Key.Replace("__", "-").Replace("_", ".") + ".liquid";
                    var templateValue = new Template { Description = template.Value.Description, Content = $"[file:text('{fileName}')]" };
                    await result.FileBuilder.SetFileAsync(fileName, Encoding.UTF8.GetBytes(template.Value.Content));
                    templateObjects[template.Key] = JsonSerializer.SerializeToNode(templateValue);
                }
            }
            else
            {
                foreach (var template in templates.Templates)
                {
                    templateObjects[template.Key] = JsonSerializer.SerializeToNode(template.Value);
                }
            }

            result.AddSimpleStep("AdminTemplates", "AdminTemplates", templateObjects);
        }
    }
}
