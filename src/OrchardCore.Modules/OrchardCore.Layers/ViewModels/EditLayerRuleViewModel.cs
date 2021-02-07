using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Layers.ViewModels
{
    public class EditLayerRuleViewModel
    {
        public string LayerName { get; set; }
        public string RuleId { get; set; }
        public string RuleType { get; set; }
        public dynamic Editor { get; set; }

        // [BindNever]
        // public DeploymentStep DeploymentStep { get; set; }
    }
}
