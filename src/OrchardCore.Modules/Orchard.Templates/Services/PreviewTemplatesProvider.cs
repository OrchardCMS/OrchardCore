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
        private readonly TemplatesDocument _templatesDocument;

        public PreviewTemplatesProvider(IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (!httpContext.Request.Cookies.ContainsKey("orchard:templates"))
            {
                return;
            }

            var template = JsonConvert.DeserializeObject<Template>(httpContext.Request.Cookies["orchard:templates"]);
            _templatesDocument = new TemplatesDocument();
            _templatesDocument.Templates.Add(template.Description, template);
        }

        public TemplatesDocument GetTemplates()
        {
            return _templatesDocument;
        }
    }
}