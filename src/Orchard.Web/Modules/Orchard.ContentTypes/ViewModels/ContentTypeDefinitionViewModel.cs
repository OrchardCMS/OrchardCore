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
            TypeFieldSettings = new List<dynamic>();
        }

        [BindNever]
        public ContentTypeDefinition ContentTypeDefinition { get; set; }

        /// <summary>
        /// List of shapes representing all the settings for the type.
        /// </summary>
        [BindNever]
        public dynamic TypeSettings { get; set; }

        /// <summary>
        /// List of shapes representing all the part settings for the type.
        /// </summary>
        [BindNever]
        public List<dynamic> TypePartSettings { get; set; }

        /// <summary>
        /// List of shapes representing all field settings for the type.
        /// </summary>
        [BindNever]
        public List<dynamic> TypeFieldSettings { get; set; }
    }
}