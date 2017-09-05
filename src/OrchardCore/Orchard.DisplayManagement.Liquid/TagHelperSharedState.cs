using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// Singleton containing shared state for tag helper tags.
    /// </summary>
    public class TagHelperSharedState
    {
        public List<TagHelperDescriptor> TagHelperDescriptors { get; set; }
    }
}
