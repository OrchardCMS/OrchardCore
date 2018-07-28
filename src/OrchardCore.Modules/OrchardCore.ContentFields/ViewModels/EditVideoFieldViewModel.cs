using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditYoutubeVideoFieldViewModel : YoutubeVideoFieldDisplayViewModel
    {
        [Required]
        [DataType(DataType.Url)]
        public string RawAddress { get; set; }
        public string EmbededAddress { get; set; }
    }
}
