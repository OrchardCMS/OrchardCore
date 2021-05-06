using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

            var templateObjects = new JObject();
            var templates = await _templatesManager.GetTemplatesDocumentAsync();

            if (allTemplatesStep.ExportAsFiles)
            {
                foreach (var template in templates.Templates)
                {
                    var fileName = "AdminTemplates/" + template.Key.Replace("__", "-").Replace("_", ".") + ".liquid";
                    var templateValue = new Template { Description = template.Value.Description, Content = $"[file:text('{fileName}')]" };
                    await result.FileBuilder.SetFileAsync(fileName, Encoding.UTF8.GetBytes(template.Value.Content));
                    templateObjects[template.Key] = JObject.FromObject(templateValue);
                }
            }
            else
            {
                foreach (var template in templates.Templates)
                {
                    templateObjects[template.Key] = JObject.FromObject(template.Value);
                }
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "AdminTemplates"),
                new JProperty("AdminTemplates", templateObjects)
            ));
        }
    }
}
