using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentTypes.ViewModels
{
    public class ContentTypeDefinitionViewModel
    {
        public ContentTypeDefinitionViewModel()
        {
            TypeSettings = new List<dynamic>();
            TypePartSettings = new List<dynamic>();
        }

        public string DisplayName { get; set; }

        [BindNever]
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
        [BindNever]
        public dynamic TypeSettings { get; set; }
        [BindNever]
        public List<dynamic> TypePartSettings { get; set; }
    }
}