using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;

namespace Orchard.Settings
{
    public interface ISite
    {
        string SiteName { get; set; }
        string SiteSalt { get; set; }
        string SuperUser { get; set; }
        string Culture { get; set; }
        string Calendar { get; set; }
        string TimeZone { get; set; }
        ResourceDebugMode ResourceDebugMode { get; set; }
        bool UseCdn { get; set; }
        int PageSize { get; set; }
        int MaxPageSize { get; set; }
        int MaxPagedCount { get; set; }
        string BaseUrl { get; set; }
        RouteValueDictionary HomeRoute { get; set; }
        JObject Properties { get; set; }
    }
}
