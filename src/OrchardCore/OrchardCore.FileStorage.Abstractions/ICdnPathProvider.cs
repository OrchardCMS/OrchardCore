namespace OrchardCore.FileStorage
{
    public interface ICdnPathProvider
    {
        string RemoveCdnPath(string path);
        bool MatchCdnPath(string path);
    }
}
