using System.Text;
using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Deployment;

public class AllTemplatesDeploymentSource
    : DeploymentSourceBase<AllTemplatesDeploymentStep>
{
    private readonly TemplatesManager _templatesManager;

    public AllTemplatesDeploymentSource(TemplatesManager templatesManager)
    {
        _templatesManager = templatesManager;
    }

    protected override async Task ProcessAsync(AllTemplatesDeploymentStep step, DeploymentPlanResult result)
    {
        var templateObjects = new JsonObject();
        var templates = await _templatesManager.GetTemplatesDocumentAsync();

        if (step.ExportAsFiles)
        {
            foreach (var template in templates.Templates)
            {
                var fileName = "Templates/" + template.Key.Replace("__", "-").Replace("_", ".") + ".liquid";
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

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Templates",
            ["Templates"] = templateObjects,
        });
    }
}
