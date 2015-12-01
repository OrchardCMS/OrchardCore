using Orchard.ContentManagement;

namespace Orchard.Core.Settings
{
    public interface ISite : IContent
    {
        string PageTitleSeparator { get; }
        string SiteName { get; }
        string SiteSalt { get; }
        string SuperUser { get; }
        string Culture { get; set; }
        string Calendar { get; set; }
        string TimeZone { get; }
        //ResourceDebugMode ResourceDebugMode { get; set; }
        bool UseCdn { get; set; }
        int PageSize { get; set; }
        int MaxPageSize { get; set; }
        int MaxPagedCount { get; set; }
        string BaseUrl { get; }
    }
}
