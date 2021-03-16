using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.ViewModels
{
    public class AddFieldViewModel
    {
        public AddFieldViewModel()
        {
            Fields = new List<string>();
        }

        /// <summary>
        /// The technical name of the field
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The display name of the field
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The selected field type
        /// </summary>
        [Required]
        public string FieldTypeName { get; set; }

        /// <summary>
        /// The part to add the field to
        /// </summary>
        [BindNever]
        public ContentPartDefinition Part { get; set; }

        /// <summary>
        /// List of the available Field types
        /// </summary>
        [BindNever]
        public List<string> Fields { get; set; }
    }
}
