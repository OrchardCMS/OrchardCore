namespace OrchardCore.Layers.ViewModels
{
    public class LayerRuleEditViewModel
    {
        public string Name { get; set; }
        public string ConditionGroupId { get; set; }
        public string ConditionId { get; set; }
        public string ConditionType { get; set; }
        public dynamic Editor { get; set; }
    }
}
