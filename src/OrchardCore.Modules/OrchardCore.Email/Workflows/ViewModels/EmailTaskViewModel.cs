namespace OrchardCore.Email.Workflows.ViewModels;

public class EmailTaskViewModel
{
    public string AuthorExpression { get; set; }

    public string SenderExpression { get; set; }

    public string ReplyToExpression { get; set; }

    public string CcExpression { get; set; }

    public string BccExpression { get; set; }

    public string RecipientsExpression { get; set; }

    public string SubjectExpression { get; set; }

    public MailMessageBodyFormat BodyFormat { get; set; }

    public string TextBody { get; set; }

    public string HtmlBody { get; set; }
}
