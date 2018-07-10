using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class FormSubmittedContext
    {
        public FormSubmittedContext(string formId, IFormCollection formCollection, Func<Task<ContentItem>> formLoader)
        {
            FormId = formId;
            FormCollection = formCollection;
            FormContentItem = new Lazy<Task<ContentItem>>(formLoader);
        }

        public string FormId { get; }
        public IFormCollection FormCollection { get; }
        public Lazy<Task<ContentItem>> FormContentItem { get; }
    }
}