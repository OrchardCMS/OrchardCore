using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Contents.ViewModels
{
    public class ListContentTypesViewModel
    {
        public IEnumerable<ContentTypeDefinition> Types { get; set; }
    }
}