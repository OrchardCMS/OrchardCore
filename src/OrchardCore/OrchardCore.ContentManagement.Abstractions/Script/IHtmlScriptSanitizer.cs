namespace OrchardCore.ContentManagement.Script
{
    public interface IHtmlScriptSanitizer
    {
        string Sanitize(string html);
    }
}
