namespace OrchardCore.Infrastructure.Script
{
    public interface IHtmlScriptSanitizer
    {
        string Sanitize(string html);
    }
}
