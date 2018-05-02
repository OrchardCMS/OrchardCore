using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class FormPart : ContentPart
    {
        public string Action { get; set; }
        public string Method { get; set; }
    }
}
