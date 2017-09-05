namespace OrchardCore.Localization
{
    public struct PluralizationArgument
    {
        public int Count { get; set; }
        public string[] Forms { get; set; }
        public object[] Arguments { get; set; }
    }
}
