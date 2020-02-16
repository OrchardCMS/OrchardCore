using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Forms.Workflows.ViewModels
{
    public class ValidateFormFieldTaskViewModel
    {
        [Required]
        public string FieldName { get; set; }

        [Required]
        public string ErrorMessage { get; set; }
    }
}
