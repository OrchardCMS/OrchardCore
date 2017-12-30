using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels
{
    public class EmailTaskViewModel
    {
        public string SenderExpression { get; set; }

        [Required]
        public string RecipientsExpression { get; set; }
        public string SubjectExpression { get; set; }
        public string Body { get; set; }
    }
}
