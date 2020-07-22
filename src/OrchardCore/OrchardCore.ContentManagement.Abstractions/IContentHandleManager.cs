using System;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public interface IContentHandleManager
    {
        Task<string> GetContentItemIdAsync(string alias);
    }

    /// <summary>
    /// This interface has been renamed to IContentHandleManager
    /// and will be removed in a future release.
    /// </summary>
    public interface IContentAliasManager : IContentHandleManager {}
}
