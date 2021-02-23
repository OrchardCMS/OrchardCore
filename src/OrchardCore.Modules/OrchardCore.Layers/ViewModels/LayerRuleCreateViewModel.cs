namespace OrchardCore.Layers.ViewModels
{
    public class LayerRuleCreateViewModel
    {
        public string Name { get; set; }
        public string ConditionGroupId { get; set; }
        public string ConditionType { get; set; }
        public bool Admin { get; set; }
        public dynamic Editor { get; set; }
    }
}
