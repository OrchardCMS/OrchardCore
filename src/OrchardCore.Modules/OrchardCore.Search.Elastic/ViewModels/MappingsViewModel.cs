using System.Text.Json;

namespace OrchardCore.Search.Elastic.ViewModels
{
    internal class MappingsViewModel
    {
        public string IndexName { get; set; }
        public string Mappings { get; set; }
    }
}
