using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class VideoFieldDisplayViewModel
    {
        public VideoField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
