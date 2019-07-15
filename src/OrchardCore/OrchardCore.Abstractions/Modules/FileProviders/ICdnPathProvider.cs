namespace OrchardCore.Abstractions.Modules.FileProviders
{
    public interface ICdnPathProvider
    {
        string RemoveCdnPath(string path);
        bool MatchCdnPath(string path);
    }
}
