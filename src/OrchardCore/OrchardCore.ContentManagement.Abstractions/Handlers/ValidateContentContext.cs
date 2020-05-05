namespace OrchardCore.ContentManagement.Handlers
{
    public class ValidateContentContext : ContentContextBase
    {
        public ValidateContentContext(ContentItem contentItem) : base(contentItem)
        {
        }

        public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();
    }

    public static class ValidateContentContextExtensions
    {
        public static void Fail(this ValidateContentContext context, params string[] errors)
        {
            context.ContentValidateResult.Fail(errors);
        }
    }
}
