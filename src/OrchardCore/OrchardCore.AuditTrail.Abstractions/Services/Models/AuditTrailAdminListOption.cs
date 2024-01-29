using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.ViewModels;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailAdminListOption
    {
        public AuditTrailAdminListOption(
            string value,
            Func<string, IQuery<AuditTrailEvent>, QueryExecutionContext<AuditTrailEvent>, ValueTask<IQuery<AuditTrailEvent>>> query,
            Func<IServiceProvider, AuditTrailAdminListOption, AuditTrailIndexOptions, SelectListItem> selectListItem,
            bool isDefault)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Query = query ?? throw new ArgumentNullException(nameof(query));
            SelectListItem = selectListItem;
            IsDefault = isDefault;
        }

        public string Value { get; }
        public Func<string, IQuery<AuditTrailEvent>, QueryExecutionContext<AuditTrailEvent>, ValueTask<IQuery<AuditTrailEvent>>> Query { get; }
        public Func<IServiceProvider, AuditTrailAdminListOption, AuditTrailIndexOptions, SelectListItem> SelectListItem { get; }
        public bool IsDefault { get; }
    }
}
