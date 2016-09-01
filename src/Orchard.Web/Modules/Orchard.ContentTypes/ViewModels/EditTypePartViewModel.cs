using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentTypes.ViewModels
{
    public class EditTypePartViewModel
    {
        /// <summary>
        /// The technical name of the part
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The display name of the part
        /// </summary>
        public string DisplayName { get; set; }

        public string Description { get; set; }

        [BindNever]
        public ContentTypePartDefinition TypePartDefinition { get; set; }

        [BindNever]
        public dynamic Editor { get; set; }
    }
}