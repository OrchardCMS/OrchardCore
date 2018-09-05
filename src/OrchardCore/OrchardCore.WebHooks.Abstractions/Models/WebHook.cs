using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.WebHooks.Abstractions.Models
{
    public class WebHook
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string HttpMethod { get; set; } = HttpMethods.Post;
        
        [Required]
        [Url(ErrorMessage = "Invalid webhook URL.")]
        public string Url { get; set; }

        [Required] 
        public string ContentType { get; set; }

        public IList<KeyValuePair<string, string>> Headers { get; set; } = new List<KeyValuePair<string, string>>();

        [Required]
        [StringLength(64, MinimumLength = 32, ErrorMessage = "Secret must be at least 32 characters and less than 64.")]
        public string Secret { get; set; }

        public ISet<string> Events { get; set; } = new HashSet<string>();

        public string PayloadTemplate { get; set; }

        public bool Enabled { get; set; } = true;

        public bool ValidateSsl { get; set; } = true;
    }
}