namespace OrchardCore.ContentFields.Settings
{
    public class NumericFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public int Scale { get; set; }
        public decimal? Minimum { get; set; }
        public decimal? Maximum { get; set; }
        public string Placeholder { get; set; }
        public string DefaultValue { get; set; }
    }
}
