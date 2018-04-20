using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class SetPropertyTaskViewModel
    {
        [Required]
        public string PropertyName { get; set; }

        public string Value { get; set; }
    }
}
