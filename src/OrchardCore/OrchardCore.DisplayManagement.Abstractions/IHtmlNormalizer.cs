namespace OrchardCore.DisplayManagement;

public interface IHtmlNormalizer
{
    string Normalize(string html, bool sanitize = false);
}
