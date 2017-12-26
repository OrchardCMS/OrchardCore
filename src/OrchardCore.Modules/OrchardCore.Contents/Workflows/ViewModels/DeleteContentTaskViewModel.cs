using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class DeleteContentTaskViewModel
    {
        /// <summary>
        /// The name of the workflow parameter containing a reference to the content item to be deleted.
        /// </summary>
        public string ContentParameterName { get; set; }

        /// <summary>
        /// The available parameters as defined on the workflow definition.
        /// </summary>
        public IList<SelectListItem> AvailableParameters { get; set; } = new List<SelectListItem>();
    }
}
