using System.ComponentModel.DataAnnotations;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Email.ViewModels
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
