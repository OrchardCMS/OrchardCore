namespace OrchardCore.Infrastructure.Html
{
    public interface IHtmlSanitizerService
    {
        string Sanitize(string html);
    }
}
