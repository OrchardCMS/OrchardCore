using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services
{
    /// <summary>
    /// And instance of this class provides custom templates to use while previewing a page.
    /// </summary>
    public class PreviewTemplatesProvider
    {
        private readonly Lazy<TemplatesDocument> _templatesDocument;

        public PreviewTemplatesProvider(IHttpContextAccessor httpContextAccessor)
        {
            _templatesDocument = new Lazy<TemplatesDocument>(() =>
            {

                var httpContext = httpContextAccessor.HttpContext;

                if (!httpContext.Request.Cookies.ContainsKey("orchard:templates:count"))
                {
                    return null;
                }

                var sb = new StringBuilder();
                int.TryParse(httpContext.Request.Cookies["orchard:templates:count"], out int count);
                for(var i = 0; i < count; i++)
                {
                    var chunk = httpContext.Request.Cookies["orchard:templates:" + i];
                    sb.Append(chunk);
                }

                var content = Encoding.UTF8.GetString(Convert.FromBase64String(sb.ToString()));
                var template = JsonConvert.DeserializeObject<Template>(content);
                var templatesDocument = new TemplatesDocument();

                if (template == null || template.Description == null)
                {
                    // An error occured while deserializing
                    return templatesDocument;
                }

                templatesDocument.Templates.Add(template.Description, template);

                return templatesDocument;
            });

        }

        public TemplatesDocument GetTemplates()
        {
            return _templatesDocument.Value;
        }
    }
}