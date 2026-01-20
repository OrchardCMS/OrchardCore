using System.Collections.Generic;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Contents.ViewModels
{
    public class ListContentTypesViewModel
    {
        public IEnumerable<ContentTypeDefinition> Types { get; set; }
    }
}
