using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Entities;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Rules;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentSource : IDeploymentSource
    {
        private readonly ILayerService _layerService;
        private readonly ISiteService _siteService;
        private readonly ConditionOptions _options;
        private readonly IEnumerable<IConditionFactory> _factories;

        public AllLayersDeploymentSource(
            ILayerService layerService, 
            ISiteService siteService,
            IOptions<ConditionOptions> options,
            IEnumerable<IConditionFactory> factories)
        {
            _layerService = layerService;
            _siteService = siteService;
            _options = options.Value;
            _factories = factories;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allLayersStep = step as AllLayersDeploymentStep;

            if (allLayersStep == null)
            {
                return;
            }

            var layers = await _layerService.GetLayersAsync();
            var factories = _factories.ToDictionary(x => x.GetType());

            var layerSteps = layers.Layers.Select(layer => new LayerStepModel
            {
                Name = layer.Name,
                Rule = null,
                Description = layer.Description,
                LayerRule = new RuleStepModel
                {
                    ConditionId = layer.LayerRule.ConditionId,
                    Conditions = layer.LayerRule.Conditions.Select(condition => new ConditionStepModel
                    {
                        Name = factories[_options.Factories[condition.GetType()]].Name,
                        Condition = condition
                    }).ToArray()
                }
            });
            
            result.Steps.Add(new JObject(
                new JProperty("name", "Layers"),
                new JProperty("Layers", JArray.FromObject(layerSteps))
            ));

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // Adding Layer settings
            result.Steps.Add(new JObject(
                new JProperty("name", "Settings"),
                new JProperty("LayerSettings", JObject.FromObject(siteSettings.As<LayerSettings>()))
            ));
        }
    }

    public class LayerStepModel
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public string Description { get; set; }

        public RuleStepModel LayerRule { get; set; }
    }

    public class RuleStepModel
    {
        public string ConditionId { get; set; }
        public ConditionStepModel[] Conditions { get; set; }
    }

    public class ConditionStepModel
    {
        public string Name { get; set; }
        public Condition Condition { get; set; }
    }    
}
