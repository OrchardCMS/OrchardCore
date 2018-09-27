using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditFontIconPickerFieldViewModel : FontIconPickerFieldViewModel
    {
        [Required(ErrorMessage = "Font Icon field is required")]
        public string IconCode { get; set; }
        public string IconFormatted { get; set; }
    }
}
