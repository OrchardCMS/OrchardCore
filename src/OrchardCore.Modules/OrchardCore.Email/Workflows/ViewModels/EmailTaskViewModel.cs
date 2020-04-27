using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Email.Workflows.ViewModels
{
    public class EmailTaskViewModel
    {
        public string SenderExpression { get; set; }

        public string AuthorExpression { get; set; }

        [Required]
        public string RecipientsExpression { get; set; }

        public string SubjectExpression { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
