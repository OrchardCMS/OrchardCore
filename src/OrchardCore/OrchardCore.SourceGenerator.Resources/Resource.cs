namespace OrchardCore.SourceGenerator.Resources
{
    public class Resource
    {
        public string Name { get; set; }

        public string[] Dependencies { get; set; }

        public string Url { get; set; }

        public string Cdn { get; set; }

        public string[] CdnIntegrity { get; set; }

        public string Version { get; set; }

        public ResourceType Type { get; set; }
    }
}
