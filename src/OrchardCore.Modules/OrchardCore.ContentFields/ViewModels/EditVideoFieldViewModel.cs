using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditYoutubeVideoFieldViewModel : YoutubeVideoFieldDisplayViewModel
    {
        public string Address { get; set; }
    }
}
