using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditYoutubeFieldViewModel : YoutubeFieldDisplayViewModel
    {
        [DataType(DataType.Url, ErrorMessage = "The field only accepts Urls")]
        public string RawAddress { get; set; }
        public string EmbeddedAddress { get; set; }
    }
}
