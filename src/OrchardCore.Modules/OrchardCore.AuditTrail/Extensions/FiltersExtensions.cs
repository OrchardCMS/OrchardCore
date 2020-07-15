using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Extensions
{
    public static class FiltersExtensions
    {
        public static string Get(this Filters filters, string key) => !filters.ContainsKey(key) ? null : filters[key];
    }
}
