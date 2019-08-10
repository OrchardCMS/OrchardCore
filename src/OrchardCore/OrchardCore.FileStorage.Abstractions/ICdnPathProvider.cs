namespace OrchardCore.FileStorage
{
    /// <summary>
    /// Provides path mappings when using a Cdn.
    /// </summary>
    public interface ICdnPathProvider
    {
        string RemoveCdnPath(string path);
        bool MatchCdnPath(string path);
    }
}
