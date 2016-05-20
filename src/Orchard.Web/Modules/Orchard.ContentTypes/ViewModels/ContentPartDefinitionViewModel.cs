using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentTypes.ViewModels
{
    public class ContentPartDefinitionViewModel
    {
        public ContentPartDefinitionViewModel()
        {
            PartFieldSettings = new List<dynamic>();
        }

        [BindNever]
        public ContentPartDefinition ContentPartDefinition { get; set; }

        /// <summary>
        /// List of shapes representing all the settings for the part.
        /// </summary>
        [BindNever]
        public dynamic PartSettings { get; set; }

        /// <summary>
        /// List of shapes representing all field settings for the part.
        /// </summary>
        [BindNever]
        public List<dynamic> PartFieldSettings { get; set; }
    }
}