using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Providers
{
    public abstract class AuditTrailEventProviderBase : IAuditTrailEventProvider
    {
        public abstract void Describe(DescribeContext context);

        public IStringLocalizer T { get; set; }
    }
}
