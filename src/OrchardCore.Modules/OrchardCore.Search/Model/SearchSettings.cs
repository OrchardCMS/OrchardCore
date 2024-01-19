using Microsoft.Extensions.Options;

namespace OrchardCore.Search.Models
{
    public class SearchSettings : IAsyncOptions
    {
        public string ProviderName { get; set; }

        public string Placeholder { get; set; }

        public string PageTitle { get; set; }
    }
}
