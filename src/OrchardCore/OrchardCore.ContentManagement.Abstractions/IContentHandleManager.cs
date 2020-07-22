using System;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public interface IContentHandleManager
    {
        Task<string> GetContentItemIdAsync(string alias);
    }

    [Obsolete("Use IContentHandleManager instead.")]
    public interface IContentAliasManager : IContentHandleManager {}
}
