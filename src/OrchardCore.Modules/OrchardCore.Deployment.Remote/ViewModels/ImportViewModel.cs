using Microsoft.AspNetCore.Http;

namespace OrchardCore.Deployment.Remote.ViewModels
{
    public class ImportViewModel
    {
        public string ClientName { get; set; }
        public string ApiKey { get; set; }
        public IFormFile Content { get; set; }
    }
}
