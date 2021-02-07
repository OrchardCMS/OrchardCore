using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Layers.ViewModels
{
    public class LayerRuleEditViewModel
    {
        public string Name { get; set; }
        public string RuleGroupId { get; set; }
        public string RuleId { get; set; }
        public string RuleType { get; set; }
        public dynamic Editor { get; set; }

        // [BindNever]
        // public DeploymentStep DeploymentStep { get; set; }
    }
}
