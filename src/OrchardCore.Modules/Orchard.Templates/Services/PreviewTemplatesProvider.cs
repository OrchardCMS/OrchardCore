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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreviewTemplatesProvider(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public TemplatesDocument GetTemplates()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (!httpContext.Request.Cookies.ContainsKey("orchard:templates"))
            {
                return null;
            }

            var template = JsonConvert.DeserializeObject<Template>(httpContext.Request.Cookies["orchard:templates"]);
            var templatesDocument = new TemplatesDocument();
            templatesDocument.Templates.Add(template.Description, template);

            return templatesDocument;
        }
    }
}