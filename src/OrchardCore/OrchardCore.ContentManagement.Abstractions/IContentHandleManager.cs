using System;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement
{
    public interface IContentHandleManager
    {
        Task<string> GetContentItemIdAsync(string handle);
    }

    [Obsolete("This interface has been renamed to IContentHandleManager")]
    public interface IContentAliasManager { }
}
