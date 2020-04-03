using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Lists.Models
{
    /// <summary>
    /// Attached to a content item instance when it's part of a list.
    /// </summary>
    public class ContainedPart : ContentPart
    {
        /// <summary>
        /// The content item id of the list owning this content item.
        /// </summary>
        public string ListContentItemId { get; set; }

        /// <summary>
        /// The order of this content item in the list.
        /// </summary>
        public int Order { get; set; }
    }
}
