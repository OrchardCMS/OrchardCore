using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpRequestTaskViewModel
    {
        [Required]
        public string Url { get; set; }

        [Required]
        public string HttpMethod { get; set; }

        public string Headers { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; }
        public string HttpResponseCodes { get; set; }
    }
}
